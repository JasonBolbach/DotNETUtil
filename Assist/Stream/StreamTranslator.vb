Imports System.Threading
Imports System.Threading.Tasks

''' <summary>
''' Copies one stream to another while passing it through an optional compression stage and/or an optional
''' encryption stage, hashing (SHA-256) and counting the bytes on both sides.
''' </summary>
''' <remarks>
''' <para>Write: Original -> compress -> encrypt -> Stored.   Read: Stored -> decrypt -> decompress -> Original.</para>
''' <para>The caller owns every stream and the ICryptoTransform: none of them are closed or disposed here. After a
''' call the streams are left open, positioned wherever the copy ended; encrypted/compressed output has been fully
''' finalized and flushed.</para>
''' <para>Lengths are counted as bytes pass through, so they are correct for non-seekable streams too.</para>
''' <para>Feedback is given through the CopyProgress event and/or an IProgress. The async methods may raise the
''' CopyProgress event on a thread-pool thread; prefer IProgress there (Progress(Of T) marshals to the UI thread).</para>
''' </remarks>
Public Class StreamTranslator
    ''' <summary>Raised as the copy proceeds. Progress is -1 when the input length is unknown.</summary>
    Public Event CopyProgress(sender As Object, e As CopyProgressEventArgs)

    Private pBlockSize As Integer = 1024 * 64
    'When the input length is unknown, report roughly once per this many input bytes
    Private Const UnknownLengthReportBytes As Long = 1024 * 1024

    Public Enum CompressionKind
        None
        GZip
        Deflate
    End Enum

    Public Class TranslationResults
        Public ReadOnly Property OriginalHash As String
        Public ReadOnly Property StoredHash As String
        Public ReadOnly Property OriginalLength As Long
        Public ReadOnly Property StoredLength As Long
        Public Sub New(OH As String, SH As String, OL As Long, SL As Long)
            Me.OriginalHash = OH : Me.StoredHash = SH
            Me.OriginalLength = OL : Me.StoredLength = SL
        End Sub
        Public Overrides Function ToString() As String
            Return "O:(" & OriginalLength.ToString() & "," & OriginalHash & ") S:(" & StoredLength.ToString() & "," & StoredHash & ")"
        End Function
    End Class

    Public Class CopyProgressEventArgs
        ''' <summary>Total input length, or -1 if unknown.</summary>
        Public ReadOnly Property InputLength As Long
        ''' <summary>Bytes produced so far (may lag while a compressor/encryptor is buffering).</summary>
        Public ReadOnly Property OutputLength As Long
        ''' <summary>Input bytes consumed so far.</summary>
        Public ReadOnly Property TotalTranslated As Long
        ''' <summary>Percent complete (0-100), or -1 if the input length is unknown.</summary>
        Public ReadOnly Property Progress As Double
        Public Sub New(IL As Long, OL As Long, TT As Long, P As Double)
            InputLength = IL : OutputLength = OL
            TotalTranslated = TT : Progress = P
        End Sub
        Public Overrides Function ToString() As String
            Return String.Format("({0:0.00}%) ({1}/{2}/{3})", Progress, InputLength, TotalTranslated, OutputLength)
        End Function
    End Class

    ''' <summary>Size of the copy buffer in bytes (must be at least 1).</summary>
    Public Property BlockSize() As Integer
        Set(value As Integer)
            If value < 1 Then Throw New ArgumentOutOfRangeException(NameOf(value), "BlockSize must be at least 1")
            pBlockSize = value
        End Set
        Get
            Return pBlockSize
        End Get
    End Property

    Public Sub New()
    End Sub

#Region "Write (Original -> Stored)"
    ''' <summary>Reads OriginalStream to its end, writing it compressed and/or encrypted to StoredStream.</summary>
    Public Function Write(OriginalStream As IO.Stream, StoredStream As IO.Stream, Compression As CompressionKind, CryptoTransform As Security.Cryptography.ICryptoTransform, Optional Progress As IProgress(Of CopyProgressEventArgs) = Nothing) As TranslationResults
        CheckArguments(OriginalStream, StoredStream, Compression)

        Using OriginalDigest As New Digest(), StoredDigest As New Digest()
            Dim Tracker As New ProgressTracker(InputLengthOf(OriginalStream))
            Raise(Tracker.Start(), Progress)
            Dim Buffer(pBlockSize - 1) As Byte

            Using StoredTap As New TapStream(StoredStream, StoredDigest)
                Using Crypto As Security.Cryptography.CryptoStream = CreateEncryptor(StoredTap, CryptoTransform)
                    Using Compressor As IO.Stream = CreateCompressor(If(Crypto Is Nothing, CType(StoredTap, IO.Stream), Crypto), Compression)
                        Dim Sink As IO.Stream = If(Compressor Is Nothing, If(Crypto Is Nothing, CType(StoredTap, IO.Stream), Crypto), Compressor)
                        Do
                            Dim BytesRead As Integer = OriginalStream.Read(Buffer, 0, Buffer.Length)
                            If BytesRead <= 0 Then Exit Do
                            OriginalDigest.Append(Buffer, 0, BytesRead)
                            Sink.Write(Buffer, 0, BytesRead)
                            Raise(Tracker.Update(OriginalDigest.Count, StoredDigest.Count), Progress)
                        Loop
                    End Using 'Compressor: writes the compression trailer
                End Using 'Crypto: writes the final cipher block
            End Using
            StoredStream.Flush()

            Raise(Tracker.Finish(OriginalDigest.Count, StoredDigest.Count), Progress)
            Return New TranslationResults(OriginalDigest.Finish(), StoredDigest.Finish(), OriginalDigest.Count, StoredDigest.Count)
        End Using
    End Function

    ''' <summary>Asynchronous version of Write.</summary>
    Public Async Function WriteAsync(OriginalStream As IO.Stream, StoredStream As IO.Stream, Compression As CompressionKind, CryptoTransform As Security.Cryptography.ICryptoTransform, Optional Progress As IProgress(Of CopyProgressEventArgs) = Nothing, Optional Token As CancellationToken = Nothing) As Task(Of TranslationResults)
        CheckArguments(OriginalStream, StoredStream, Compression)

        Using OriginalDigest As New Digest(), StoredDigest As New Digest()
            Dim Tracker As New ProgressTracker(InputLengthOf(OriginalStream))
            Raise(Tracker.Start(), Progress)
            Dim Buffer(pBlockSize - 1) As Byte

            Using StoredTap As New TapStream(StoredStream, StoredDigest)
                Using Crypto As Security.Cryptography.CryptoStream = CreateEncryptor(StoredTap, CryptoTransform)
                    Using Compressor As IO.Stream = CreateCompressor(If(Crypto Is Nothing, CType(StoredTap, IO.Stream), Crypto), Compression)
                        Dim Sink As IO.Stream = If(Compressor Is Nothing, If(Crypto Is Nothing, CType(StoredTap, IO.Stream), Crypto), Compressor)
                        Do
                            Token.ThrowIfCancellationRequested()
                            Dim BytesRead As Integer = Await OriginalStream.ReadAsync(Buffer, 0, Buffer.Length, Token).ConfigureAwait(False)
                            If BytesRead <= 0 Then Exit Do
                            OriginalDigest.Append(Buffer, 0, BytesRead)
                            Await Sink.WriteAsync(Buffer, 0, BytesRead, Token).ConfigureAwait(False)
                            Raise(Tracker.Update(OriginalDigest.Count, StoredDigest.Count), Progress)
                        Loop
                    End Using
                End Using
            End Using
            Await StoredStream.FlushAsync(Token).ConfigureAwait(False)

            Raise(Tracker.Finish(OriginalDigest.Count, StoredDigest.Count), Progress)
            Return New TranslationResults(OriginalDigest.Finish(), StoredDigest.Finish(), OriginalDigest.Count, StoredDigest.Count)
        End Using
    End Function
#End Region

#Region "Read (Stored -> Original)"
    ''' <summary>
    ''' Reads InputStream (stored form) to its end, writing the decrypted and/or decompressed data to OutputStream.
    ''' A bad key, bad padding or corrupt compressed data throws CryptographicException / InvalidDataException
    ''' (with the original exception as InnerException).
    ''' </summary>
    Public Function Read(InputStream As IO.Stream, OutputStream As IO.Stream, Compression As CompressionKind, CryptoTransform As Security.Cryptography.ICryptoTransform, Optional Progress As IProgress(Of CopyProgressEventArgs) = Nothing) As TranslationResults
        CheckArguments(InputStream, OutputStream, Compression)

        Using InputDigest As New Digest(), OutputDigest As New Digest()
            Dim Tracker As New ProgressTracker(InputLengthOf(InputStream))
            Raise(Tracker.Start(), Progress)
            Dim Buffer(pBlockSize - 1) As Byte

            Try
                Using InputTap As New TapStream(InputStream, InputDigest)
                    Using Crypto As Security.Cryptography.CryptoStream = CreateDecryptor(InputTap, CryptoTransform)
                        Using Decompressor As IO.Stream = CreateDecompressor(If(Crypto Is Nothing, CType(InputTap, IO.Stream), Crypto), Compression)
                            Dim Source As IO.Stream = If(Decompressor Is Nothing, If(Crypto Is Nothing, CType(InputTap, IO.Stream), Crypto), Decompressor)
                            Do
                                Dim BytesRead As Integer = Source.Read(Buffer, 0, Buffer.Length)
                                If BytesRead <= 0 Then Exit Do
                                OutputDigest.Append(Buffer, 0, BytesRead)
                                OutputStream.Write(Buffer, 0, BytesRead)
                                Raise(Tracker.Update(InputDigest.Count, OutputDigest.Count), Progress)
                            Loop
                        End Using
                    End Using
                End Using
            Catch ex As Security.Cryptography.CryptographicException
                Throw New Security.Cryptography.CryptographicException("Decryption error: " & ex.Message, ex)
            Catch ex As IO.InvalidDataException
                Throw New IO.InvalidDataException("Decompression error: " & ex.Message, ex)
            End Try
            OutputStream.Flush()

            Raise(Tracker.Finish(InputDigest.Count, OutputDigest.Count), Progress)
            Return New TranslationResults(OutputDigest.Finish(), InputDigest.Finish(), OutputDigest.Count, InputDigest.Count)
        End Using
    End Function

    ''' <summary>Asynchronous version of Read.</summary>
    Public Async Function ReadAsync(InputStream As IO.Stream, OutputStream As IO.Stream, Compression As CompressionKind, CryptoTransform As Security.Cryptography.ICryptoTransform, Optional Progress As IProgress(Of CopyProgressEventArgs) = Nothing, Optional Token As CancellationToken = Nothing) As Task(Of TranslationResults)
        CheckArguments(InputStream, OutputStream, Compression)

        Using InputDigest As New Digest(), OutputDigest As New Digest()
            Dim Tracker As New ProgressTracker(InputLengthOf(InputStream))
            Raise(Tracker.Start(), Progress)
            Dim Buffer(pBlockSize - 1) As Byte

            Try
                Using InputTap As New TapStream(InputStream, InputDigest)
                    Using Crypto As Security.Cryptography.CryptoStream = CreateDecryptor(InputTap, CryptoTransform)
                        Using Decompressor As IO.Stream = CreateDecompressor(If(Crypto Is Nothing, CType(InputTap, IO.Stream), Crypto), Compression)
                            Dim Source As IO.Stream = If(Decompressor Is Nothing, If(Crypto Is Nothing, CType(InputTap, IO.Stream), Crypto), Decompressor)
                            Do
                                Token.ThrowIfCancellationRequested()
                                Dim BytesRead As Integer = Await Source.ReadAsync(Buffer, 0, Buffer.Length, Token).ConfigureAwait(False)
                                If BytesRead <= 0 Then Exit Do
                                OutputDigest.Append(Buffer, 0, BytesRead)
                                Await OutputStream.WriteAsync(Buffer, 0, BytesRead, Token).ConfigureAwait(False)
                                Raise(Tracker.Update(InputDigest.Count, OutputDigest.Count), Progress)
                            Loop
                        End Using
                    End Using
                End Using
            Catch ex As Security.Cryptography.CryptographicException
                Throw New Security.Cryptography.CryptographicException("Decryption error: " & ex.Message, ex)
            Catch ex As IO.InvalidDataException
                Throw New IO.InvalidDataException("Decompression error: " & ex.Message, ex)
            End Try
            Await OutputStream.FlushAsync(Token).ConfigureAwait(False)

            Raise(Tracker.Finish(InputDigest.Count, OutputDigest.Count), Progress)
            Return New TranslationResults(OutputDigest.Finish(), InputDigest.Finish(), OutputDigest.Count, InputDigest.Count)
        End Using
    End Function
#End Region

#Region "Helpers"
    Private Shared Sub CheckArguments(Source As IO.Stream, Destination As IO.Stream, Compression As CompressionKind)
        If Source Is Nothing Then Throw New ArgumentNullException(NameOf(Source))
        If Destination Is Nothing Then Throw New ArgumentNullException(NameOf(Destination))
        If Not Source.CanRead Then Throw New ArgumentException("The source stream is not readable", NameOf(Source))
        If Not Destination.CanWrite Then Throw New ArgumentException("The destination stream is not writable", NameOf(Destination))
        If Not [Enum].IsDefined(GetType(CompressionKind), Compression) Then Throw New ArgumentOutOfRangeException(NameOf(Compression))
    End Sub

    ''' <summary>Bytes remaining in a seekable stream, or -1 if the length is unknown.</summary>
    Private Shared Function InputLengthOf(S As IO.Stream) As Long
        Try
            If S.CanSeek Then Return Math.Max(0L, S.Length - S.Position)
        Catch ex As Exception When TypeOf ex Is NotSupportedException OrElse TypeOf ex Is ObjectDisposedException
        End Try
        Return -1
    End Function

    Private Shared Function CreateEncryptor(Target As IO.Stream, Transform As Security.Cryptography.ICryptoTransform) As Security.Cryptography.CryptoStream
        If Transform Is Nothing Then Return Nothing
        Return New Security.Cryptography.CryptoStream(Target, Transform, Security.Cryptography.CryptoStreamMode.Write, True)
    End Function
    Private Shared Function CreateDecryptor(Source As IO.Stream, Transform As Security.Cryptography.ICryptoTransform) As Security.Cryptography.CryptoStream
        If Transform Is Nothing Then Return Nothing
        Return New Security.Cryptography.CryptoStream(Source, Transform, Security.Cryptography.CryptoStreamMode.Read, True)
    End Function

    'Return Nothing for CompressionKind.None (the caller then uses the stream directly)
    Private Shared Function CreateCompressor(Target As IO.Stream, Compression As CompressionKind) As IO.Stream
        Select Case Compression
            Case CompressionKind.GZip
                Return New IO.Compression.GZipStream(Target, IO.Compression.CompressionMode.Compress, True)
            Case CompressionKind.Deflate
                Return New IO.Compression.DeflateStream(Target, IO.Compression.CompressionMode.Compress, True)
            Case Else
                Return Nothing
        End Select
    End Function
    Private Shared Function CreateDecompressor(Source As IO.Stream, Compression As CompressionKind) As IO.Stream
        Select Case Compression
            Case CompressionKind.GZip
                Return New IO.Compression.GZipStream(Source, IO.Compression.CompressionMode.Decompress, True)
            Case CompressionKind.Deflate
                Return New IO.Compression.DeflateStream(Source, IO.Compression.CompressionMode.Decompress, True)
            Case Else
                Return Nothing
        End Select
    End Function

    Private Sub Raise(E As CopyProgressEventArgs, Progress As IProgress(Of CopyProgressEventArgs))
        If E Is Nothing Then Return
        RaiseEvent CopyProgress(Me, E)
        If Progress IsNot Nothing Then Progress.Report(E)
    End Sub

    ''' <summary>A running SHA-256 plus a byte count.</summary>
    Private NotInheritable Class Digest
        Implements IDisposable
        Private ReadOnly pHash As Security.Cryptography.SHA256 = Security.Cryptography.SHA256.Create()
        Private pCount As Long
        Private pFinal As String

        Public ReadOnly Property Count As Long
            Get
                Return pCount
            End Get
        End Property
        Public Sub Append(Buffer() As Byte, Offset As Integer, Length As Integer)
            If Length <= 0 Then Return
            pHash.TransformBlock(Buffer, Offset, Length, Nothing, 0)
            pCount += Length
        End Sub
        ''' <summary>Completes the hash and returns it as Base64.</summary>
        Public Function Finish() As String
            If pFinal Is Nothing Then
                pHash.TransformFinalBlock(New Byte() {}, 0, 0)
                pFinal = Convert.ToBase64String(pHash.Hash)
            End If
            Return pFinal
        End Function
        Public Sub Dispose() Implements IDisposable.Dispose
            pHash.Dispose()
        End Sub
    End Class

    ''' <summary>
    ''' Pass-through stream that hashes and counts the bytes read or written through it.
    ''' Disposing it never touches the wrapped stream.
    ''' </summary>
    Private NotInheritable Class TapStream
        Inherits IO.Stream
        Private ReadOnly pInner As IO.Stream
        Private ReadOnly pDigest As Digest

        Public Sub New(Inner As IO.Stream, Digest As Digest)
            pInner = Inner
            pDigest = Digest
        End Sub

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return pInner.CanRead
            End Get
        End Property
        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return pInner.CanWrite
            End Get
        End Property
        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return False
            End Get
        End Property
        Public Overrides ReadOnly Property Length As Long
            Get
                Throw New NotSupportedException()
            End Get
        End Property
        Public Overrides Property Position As Long
            Get
                Throw New NotSupportedException()
            End Get
            Set(value As Long)
                Throw New NotSupportedException()
            End Set
        End Property
        Public Overrides Function Seek(offset As Long, origin As IO.SeekOrigin) As Long
            Throw New NotSupportedException()
        End Function
        Public Overrides Sub SetLength(value As Long)
            Throw New NotSupportedException()
        End Sub

        Public Overrides Sub Flush()
            pInner.Flush()
        End Sub
        Public Overrides Function FlushAsync(cancellationToken As CancellationToken) As Task
            Return pInner.FlushAsync(cancellationToken)
        End Function

        Public Overrides Function Read(buffer() As Byte, offset As Integer, count As Integer) As Integer
            Dim N As Integer = pInner.Read(buffer, offset, count)
            pDigest.Append(buffer, offset, N)
            Return N
        End Function
        Public Overrides Async Function ReadAsync(buffer() As Byte, offset As Integer, count As Integer, cancellationToken As CancellationToken) As Task(Of Integer)
            Dim N As Integer = Await pInner.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(False)
            pDigest.Append(buffer, offset, N)
            Return N
        End Function
        Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)
            pInner.Write(buffer, offset, count)
            pDigest.Append(buffer, offset, count)
        End Sub
        Public Overrides Async Function WriteAsync(buffer() As Byte, offset As Integer, count As Integer, cancellationToken As CancellationToken) As Task
            Await pInner.WriteAsync(buffer, offset, count, cancellationToken).ConfigureAwait(False)
            pDigest.Append(buffer, offset, count)
        End Function
    End Class

    ''' <summary>Decides when a progress notification is due.</summary>
    Private NotInheritable Class ProgressTracker
        Private ReadOnly pTotal As Long
        Private pLastPercent As Integer = 0
        Private pLastBytes As Long = 0

        Public Sub New(TotalInput As Long)
            pTotal = TotalInput
        End Sub

        Public Function Start() As CopyProgressEventArgs
            Return New CopyProgressEventArgs(pTotal, 0, 0, If(pTotal >= 0, 0.0, -1.0))
        End Function

        ''' <summary>Returns the event to raise, or Nothing if no notification is due yet.</summary>
        Public Function Update(Translated As Long, Output As Long) As CopyProgressEventArgs
            If pTotal > 0 Then
                Dim Percent As Double = Math.Min(100.0, Translated * 100.0 / pTotal)
                Dim Whole As Integer = CInt(Math.Floor(Percent))
                If Whole <= pLastPercent Then Return Nothing
                pLastPercent = Whole
                Return New CopyProgressEventArgs(pTotal, Output, Translated, Percent)
            ElseIf pTotal < 0 Then
                If Translated - pLastBytes < UnknownLengthReportBytes Then Return Nothing
                pLastBytes = Translated
                Return New CopyProgressEventArgs(pTotal, Output, Translated, -1.0)
            End If
            Return Nothing
        End Function

        Public Function Finish(Translated As Long, Output As Long) As CopyProgressEventArgs
            Return New CopyProgressEventArgs(pTotal, Output, Translated, 100.0)
        End Function
    End Class
#End Region
End Class
