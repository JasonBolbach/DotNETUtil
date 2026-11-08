Imports System.Security.Cryptography

Public Class CA
    Public Class RSA
        Implements IDisposable

        Private pRSA As New System.Security.Cryptography.RSACryptoServiceProvider

        Public ReadOnly Property PublicOnly As Boolean
            Get
                Return pRSA.PublicOnly
            End Get
        End Property

        Public Function Sign(ByVal Data As String) As String
            Dim bData() As Byte = System.Text.Encoding.UTF8.GetBytes(Data)
            Dim Signature As String = Sign(bData)
            Erase bData
            Return Signature
        End Function
        Public Function Sign(Data() As Byte) As String
            Dim bSignature() As Byte = pRSA.SignData(Data, New System.Security.Cryptography.SHA256Managed)
            Return Convert.ToBase64String(bSignature)
        End Function

        Public Function Verify(ByVal Data As String, ByVal Signature As String) As Boolean
            Dim bData() As Byte = System.Text.Encoding.UTF8.GetBytes(Data)
            Dim bSignature() As Byte = Convert.FromBase64String(Signature)
            Dim Result As Boolean = Verify(bData, bSignature)
            Erase bData, bSignature
            Return Result
        End Function
        Public Function Verify(ByVal Data() As Byte, ByVal Signature() As Byte) As Boolean
            Return pRSA.VerifyData(Data, New System.Security.Cryptography.SHA256Managed, Signature)
        End Function

        Public Sub ImportKey(ByVal Key As String)
            Dim Bytes() As Byte = Convert.FromBase64String(Key)
            Dim XML As String = System.Text.Encoding.UTF8.GetString(Bytes)
            pRSA.FromXmlString(XML)
        End Sub
        Public Function ExportKey(ByVal IncludePrivate As String) As String
            Dim XML As String = pRSA.ToXmlString(IncludePrivate)
            Dim Bytes() As Byte = System.Text.Encoding.UTF8.GetBytes(XML)
            Dim Bytes64 As String = Convert.ToBase64String(Bytes)
            Return Bytes64
        End Function

        Public Function Encrypt(ByVal Data As String) As String
            Dim bData(), bEncrypted() As Byte
            Try
                bData = System.Text.Encoding.UTF8.GetBytes(Data)
                bEncrypted = pRSA.Encrypt(bData, True)
                Return Convert.ToBase64String(bEncrypted)
            Catch ex As Exception
            Finally
                Erase bData, bEncrypted
                Data = ""
            End Try
            Return ""
        End Function
        Public Function Decrypt(ByVal Encrypted As String) As String
            Dim bEncrypted(), bData() As Byte
            Dim Decrypted As String
            Try
                bEncrypted = Convert.FromBase64String(Encrypted)
                bData = pRSA.Decrypt(bEncrypted, True)
                Decrypted = System.Text.Encoding.UTF8.GetString(bData)
                Return Decrypted
            Catch ex As System.Security.Cryptography.CryptographicException
                Return ""
            Finally
                Erase bEncrypted, bData
                Decrypted = ""
            End Try
        End Function

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    pRSA.Dispose()
                    pRSA = Nothing
                End If
                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
    Public Class DSA
        Implements IDisposable

        Private pDSA As New System.Security.Cryptography.DSACryptoServiceProvider

        Public ReadOnly Property PublicOnly As Boolean
            Get
                Return pDSA.PublicOnly
            End Get
        End Property

        Public Property DSA As DSACryptoServiceProvider
            Get
                Return pDSA
            End Get
            Set(value As DSACryptoServiceProvider)
                pDSA = value
            End Set
        End Property

        Public Function Sign(ByVal Data As String) As String
            Dim bData() As Byte = System.Text.Encoding.UTF8.GetBytes(Data)
            Dim Signature As String = Sign(bData)
            Erase bData
            Return Signature
        End Function
        Public Function Sign(Data() As Byte) As String
            Dim bSignature() As Byte = pDSA.SignData(Data, 0, Data.Length)
            Return Convert.ToBase64String(bSignature)
        End Function

        Public Function Verify(ByVal Data As String, ByVal Signature As String) As Boolean
            Dim bData() As Byte = System.Text.Encoding.UTF8.GetBytes(Data)
            Dim bSignature() As Byte = Convert.FromBase64String(Signature)
            Dim Result As Boolean = Verify(bData, bSignature)
            Erase bData, bSignature
            Return Result
        End Function
        Public Function Verify(ByVal Data() As Byte, ByVal Signature() As Byte) As Boolean
            Return pDSA.VerifyData(Data, Signature)
        End Function

        Public Sub ImportKey(ByVal Key As String)
            Dim Bytes() As Byte = Convert.FromBase64String(Key)
            Dim XML As String = System.Text.Encoding.UTF8.GetString(Bytes)
            pDSA.FromXmlString(XML)
        End Sub
        Public Function ExportKey(ByVal IncludePrivate As Boolean) As String
            Dim XML As String = pDSA.ToXmlString(IncludePrivate)
            Dim Bytes() As Byte = System.Text.Encoding.UTF8.GetBytes(XML)
            Dim Bytes64 As String = Convert.ToBase64String(Bytes)
            Return Bytes64
        End Function

        Public Sub New()
        End Sub
        Public Sub New(Key As String)
            ImportKey(Key)
        End Sub

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    pDSA.Dispose()
                    pDSA = Nothing
                End If
                ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
                ' TODO: set large fields to null.
            End If
            Me.disposedValue = True
        End Sub

        ' TODO: override Finalize() only if Dispose(ByVal disposing As Boolean) above has code to free unmanaged resources.
        'Protected Overrides Sub Finalize()
        '    ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
        '    Dispose(False)
        '    MyBase.Finalize()
        'End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region
    End Class
    Public Class GZip
        Implements iCompressionAssist
        Public Function CompressToBytes(ByVal Data As DataSet) As Byte() Implements iCompressionAssist.CompressToBytes
            Using msCompressed As New IO.MemoryStream()
                Using GZ As New IO.Compression.GZipStream(msCompressed, IO.Compression.CompressionMode.Compress)
                    Data.WriteXml(GZ, XmlWriteMode.WriteSchema)
                    GZ.Flush() : GZ.Close()
                End Using
                Return msCompressed.ToArray()
            End Using
        End Function
        Public Function CompressToString(ByVal Data As DataSet) As String Implements iCompressionAssist.CompressToString
            Dim bCompressed() As Byte = CompressToBytes(Data)
            Dim sCompressed As String = Convert.ToBase64String(bCompressed)

            Try
                Return sCompressed
            Catch ex As Exception
                Throw ex
                Return ""
            Finally
                Erase bCompressed
                sCompressed = ""
            End Try
        End Function

        Public Function CompressToBytes(ByVal Data As IO.Stream) As Byte() Implements iCompressionAssist.CompressToBytes
            Using msCompressed As New IO.MemoryStream()
                Using GZ As New IO.Compression.GZipStream(msCompressed, IO.Compression.CompressionMode.Compress)
                    Data.CopyTo(GZ)
                    GZ.Flush() : GZ.Close()
                End Using
                Return msCompressed.ToArray()
            End Using
        End Function
        Public Function CompressToString(ByVal Data As IO.Stream) As String Implements iCompressionAssist.CompressToString
            Dim bCompressed() As Byte = CompressToBytes(Data)
            Dim sCompressed As String = Convert.ToBase64String(bCompressed)

            Try
                Return sCompressed
            Catch ex As Exception
                Throw ex
                Return ""
            Finally
                Erase bCompressed
                sCompressed = ""
            End Try
        End Function

        Public Function CompressToBytes(ByVal Data() As Byte) As Byte() Implements iCompressionAssist.CompressToBytes
            Using msCompressed As New IO.MemoryStream()
                Using GZ As New IO.Compression.GZipStream(msCompressed, IO.Compression.CompressionMode.Compress)
                    GZ.Write(Data, 0, Data.Length)
                    GZ.Flush() : GZ.Close()
                End Using
                Return msCompressed.ToArray()
            End Using
        End Function
        Public Function CompressToString(ByVal Data() As Byte) As String Implements iCompressionAssist.CompressToString
            Dim bCompressed() As Byte = CompressToBytes(Data)
            Dim sCompressed As String = Convert.ToBase64String(bCompressed)

            Try
                Return sCompressed
            Catch ex As Exception
                Throw ex
                Return ""
            Finally
                Erase bCompressed
                sCompressed = ""
            End Try
        End Function

        Public Function CompressToBytes(ByVal Data As String) As Byte() Implements iCompressionAssist.CompressToBytes
            Dim bData() As Byte = Text.Encoding.ASCII.GetBytes(Data)
            Try
                Return CompressToBytes(bData)
            Catch ex As Exception
                Throw ex
                Return Nothing
            Finally
                Erase bData
            End Try
        End Function
        Public Function CompressToString(ByVal Data As String) As String Implements iCompressionAssist.CompressToString
            Dim bCompressed() As Byte = CompressToBytes(Data)
            Dim sCompressed As String = Convert.ToBase64String(bCompressed)

            Try
                Return sCompressed
            Catch ex As Exception
                Throw ex
                Return ""
            Finally
                Erase bCompressed
                sCompressed = ""
            End Try
        End Function

        Public Function DecompressToBytes(ByVal Compressed() As Byte) As Byte() Implements iCompressionAssist.DecompressToBytes
            Using msSmall As New IO.MemoryStream(Compressed)
                Using msBig As New IO.MemoryStream
                    Using GZ As New IO.Compression.GZipStream(msSmall, IO.Compression.CompressionMode.Decompress)
                        GZ.CopyTo(msBig)
                        GZ.Flush() : GZ.Close()
                    End Using
                    Return msBig.ToArray
                End Using
            End Using
        End Function
        Public Function DecompressToString(ByVal Compressed() As Byte) As String Implements iCompressionAssist.DecompressToString
            Dim bDecompressed() As Byte = DecompressToBytes(Compressed)

            Try
                Return Text.Encoding.ASCII.GetString(bDecompressed)
            Catch ex As Exception
                Throw ex
                Return ""
            Finally
                Erase bDecompressed
            End Try
        End Function

        Public Function DecompressToBytes(ByVal Compressed As String) As Byte() Implements iCompressionAssist.DecompressToBytes
            Dim bCompressed() As Byte = Convert.FromBase64String(Compressed)
            Dim bDecompressed() As Byte = DecompressToBytes(bCompressed)
            Try
                Return bDecompressed
            Catch ex As Exception
                Throw ex
                Return Nothing
            Finally
                Erase bCompressed, bDecompressed
            End Try
        End Function
        Public Function DecompressToString(ByVal Compressed As String) As String Implements iCompressionAssist.DecompressToString
            Dim bDecompressed() As Byte = DecompressToBytes(Compressed)
            Dim sDecompressed As String = System.Text.Encoding.ASCII.GetString(bDecompressed)
            Try
                Return sDecompressed
            Catch ex As Exception
                Throw ex
                Return Nothing
            Finally
                sDecompressed = ""
                Erase bDecompressed
            End Try
        End Function

        Public Sub DecompressToStream(ByVal Compressed As String, ByVal Output As IO.Stream) Implements iCompressionAssist.DecompressToStream
            Dim bCompressed() As Byte = Convert.FromBase64String(Compressed)
            Try
                DecompressToStream(bCompressed, Output)
            Catch ex As Exception
            Finally
                Erase bCompressed
            End Try
        End Sub
        Public Sub DecompressToStream(ByVal Compressed() As Byte, ByVal Output As IO.Stream) Implements iCompressionAssist.DecompressToStream
            Using msSmall As New IO.MemoryStream(Compressed)
                Using GZ As New IO.Compression.GZipStream(msSmall, IO.Compression.CompressionMode.Decompress)
                    GZ.CopyTo(Output)
                    GZ.Flush() : GZ.Close()
                End Using
            End Using
        End Sub

        Public Sub DecompressToDataSet(ByVal Compressed As String, ByRef DS As DataSet) Implements iCompressionAssist.DecompressToDataSet
            Dim bCompressed() As Byte = Convert.FromBase64String(Compressed)
            Try
                DecompressToDataSet(bCompressed, DS)
            Catch ex As Exception
            Finally
                Erase bCompressed
            End Try
        End Sub
        Public Sub DecompressToDataSet(ByVal Compressed() As Byte, ByRef DS As DataSet) Implements iCompressionAssist.DecompressToDataSet
            Using msCompressed As New IO.MemoryStream(Compressed)
                Using msBig As New IO.MemoryStream
                    Using GZ As New IO.Compression.GZipStream(msCompressed, IO.Compression.CompressionMode.Decompress)
                        GZ.CopyTo(msBig)
                        GZ.Flush() : GZ.Close()
                    End Using
                    msBig.Seek(0, IO.SeekOrigin.Begin)
                    DS.ReadXml(msBig)
                End Using
            End Using
        End Sub
    End Class
    Public Class Deflate
        Implements iCompressionAssist

        Public Function CompressToBytes(ByVal Data As DataSet) As Byte() Implements iCompressionAssist.CompressToBytes
            Using msCompressed As New IO.MemoryStream()
                Using Deflate As New IO.Compression.DeflateStream(msCompressed, IO.Compression.CompressionMode.Compress)
                    Data.WriteXml(Deflate, XmlWriteMode.WriteSchema)
                    Deflate.Flush() : Deflate.Close()
                End Using
                Return msCompressed.ToArray()
            End Using
        End Function
        Public Function CompressToString(ByVal Data As DataSet) As String Implements iCompressionAssist.CompressToString
            Dim bCompressed() As Byte = CompressToBytes(Data)
            Dim sCompressed As String = Convert.ToBase64String(bCompressed)

            Try
                Return sCompressed
            Catch ex As Exception
                Throw ex
                Return ""
            Finally
                Erase bCompressed
                sCompressed = ""
            End Try
        End Function

        Public Function CompressToBytes(ByVal Data As IO.Stream) As Byte() Implements iCompressionAssist.CompressToBytes
            Using msCompressed As New IO.MemoryStream()
                Using Deflate As New IO.Compression.DeflateStream(msCompressed, IO.Compression.CompressionMode.Compress)
                    Data.CopyTo(Deflate)
                    Deflate.Flush() : Deflate.Close()
                End Using
                Return msCompressed.ToArray()
            End Using
        End Function
        Public Function CompressToString(ByVal Data As IO.Stream) As String Implements iCompressionAssist.CompressToString
            Dim bCompressed() As Byte = CompressToBytes(Data)
            Dim sCompressed As String = Convert.ToBase64String(bCompressed)

            Try
                Return sCompressed
            Catch ex As Exception
                Throw ex
                Return ""
            Finally
                Erase bCompressed
                sCompressed = ""
            End Try
        End Function

        Public Function CompressToBytes(ByVal Data() As Byte) As Byte() Implements iCompressionAssist.CompressToBytes
            Using msCompressed As New IO.MemoryStream()
                Using Deflate As New IO.Compression.DeflateStream(msCompressed, IO.Compression.CompressionMode.Compress)
                    Deflate.Write(Data, 0, Data.Length)
                    Deflate.Flush() : Deflate.Close()
                End Using
                Return msCompressed.ToArray()
            End Using
        End Function
        Public Function CompressToString(ByVal Data() As Byte) As String Implements iCompressionAssist.CompressToString
            Dim bCompressed() As Byte = CompressToBytes(Data)
            Dim sCompressed As String = Convert.ToBase64String(bCompressed)

            Try
                Return sCompressed
            Catch ex As Exception
                Throw ex
                Return ""
            Finally
                Erase bCompressed
                sCompressed = ""
            End Try
        End Function

        Public Function CompressToBytes(ByVal Data As String) As Byte() Implements iCompressionAssist.CompressToBytes
            Dim bData() As Byte = Text.Encoding.ASCII.GetBytes(Data)
            Try
                Return CompressToBytes(bData)
            Catch ex As Exception
                Throw ex
                Return Nothing
            Finally
                Erase bData
            End Try
        End Function
        Public Function CompressToString(ByVal Data As String) As String Implements iCompressionAssist.CompressToString
            Dim bCompressed() As Byte = CompressToBytes(Data)
            Dim sCompressed As String = Convert.ToBase64String(bCompressed)

            Try
                Return sCompressed
            Catch ex As Exception
                Throw ex
                Return ""
            Finally
                Erase bCompressed
                sCompressed = ""
            End Try
        End Function

        Public Function DecompressToBytes(ByVal Compressed() As Byte) As Byte() Implements iCompressionAssist.DecompressToBytes
            Using msSmall As New IO.MemoryStream(Compressed)
                Using msBig As New IO.MemoryStream
                    Using Deflate As New IO.Compression.DeflateStream(msSmall, IO.Compression.CompressionMode.Decompress)
                        Deflate.CopyTo(msBig)
                        Deflate.Flush() : Deflate.Close()
                    End Using
                    Return msBig.ToArray
                End Using
            End Using
        End Function
        Public Function DecompressToString(ByVal Compressed() As Byte) As String Implements iCompressionAssist.DecompressToString
            Dim bDecompressed() As Byte = DecompressToBytes(Compressed)

            Try
                Return Text.Encoding.ASCII.GetString(bDecompressed)
            Catch ex As Exception
                Throw ex
                Return ""
            Finally
                Erase bDecompressed
            End Try
        End Function

        Public Function DecompressToBytes(ByVal Compressed As String) As Byte() Implements iCompressionAssist.DecompressToBytes
            Dim bCompressed() As Byte = Convert.FromBase64String(Compressed)
            Dim bDecompressed() As Byte = DecompressToBytes(bCompressed)
            Try
                Return bDecompressed
            Catch ex As Exception
                Throw ex
                Return Nothing
            Finally
                Erase bCompressed, bDecompressed
            End Try
        End Function
        Public Function DecompressToString(ByVal Compressed As String) As String Implements iCompressionAssist.DecompressToString
            Dim bDecompressed() As Byte = DecompressToBytes(Compressed)
            Dim sDecompressed As String = System.Text.Encoding.ASCII.GetString(bDecompressed)
            Try
                Return sDecompressed
            Catch ex As Exception
                Throw ex
                Return Nothing
            Finally
                sDecompressed = ""
                Erase bDecompressed
            End Try
        End Function

        Public Sub DecompressToStream(ByVal Compressed As String, ByVal Output As IO.Stream) Implements iCompressionAssist.DecompressToStream
            Dim bCompressed() As Byte = Convert.FromBase64String(Compressed)
            Try
                DecompressToStream(bCompressed, Output)
            Catch ex As Exception
            Finally
                Erase bCompressed
            End Try
        End Sub
        Public Sub DecompressToStream(ByVal Compressed() As Byte, ByVal Output As IO.Stream) Implements iCompressionAssist.DecompressToStream
            Using msSmall As New IO.MemoryStream(Compressed)
                Using Deflate As New IO.Compression.DeflateStream(msSmall, IO.Compression.CompressionMode.Decompress)
                    Deflate.CopyTo(Output)
                    Deflate.Flush() : Deflate.Close()
                End Using
            End Using
        End Sub

        Public Sub DecompressToDataSet(ByVal Compressed As String, ByRef DS As DataSet) Implements iCompressionAssist.DecompressToDataSet
            Dim bCompressed() As Byte = Convert.FromBase64String(Compressed)
            Try
                DecompressToDataSet(bCompressed, DS)
            Catch ex As Exception
            Finally
                Erase bCompressed
            End Try
        End Sub
        Public Sub DecompressToDataSet(ByVal Compressed() As Byte, ByRef DS As DataSet) Implements iCompressionAssist.DecompressToDataSet
            Using msCompressed As New IO.MemoryStream(Compressed)
                Using msBig As New IO.MemoryStream
                    Using Deflate As New IO.Compression.DeflateStream(msCompressed, IO.Compression.CompressionMode.Decompress)
                        Deflate.CopyTo(msBig)
                        Deflate.Flush() : Deflate.Close()
                    End Using
                    msBig.Seek(0, IO.SeekOrigin.Begin)
                    DS.ReadXml(msBig)
                End Using
            End Using
        End Sub
    End Class

    Public Class AES
        Public Shared Function EncryptString(Str As String, Key As String, IV As String) As String
            Return TransformToString(Text.Encoding.UTF8.GetBytes(Str), CreateEncryptor(Key, IV))
        End Function
        Public Shared Function DecryptString(Str As String, Key As String, IV As String) As String
            Return Text.Encoding.UTF8.GetString(TransformToBytes(Str, CreateDecryptor(Key, IV)))
        End Function
        Public Shared Function CreateEncryptor(Key As String, IV As String) As System.Security.Cryptography.ICryptoTransform
            Return CreateEncryptor(StringSHAVar(Key, 32), StringSHAVar(IV, 16))
        End Function
        Public Shared Function CreateEncryptor(Key() As Byte, IV() As Byte) As System.Security.Cryptography.ICryptoTransform
            Dim AES As New System.Security.Cryptography.AesCryptoServiceProvider
            Return AES.CreateEncryptor(Key, IV)
        End Function
        Public Shared Function CreateDecryptor(Key As String, IV As String) As System.Security.Cryptography.ICryptoTransform
            Return CreateDecryptor(StringSHAVar(Key, 32), StringSHAVar(IV, 16))
        End Function
        Public Shared Function CreateDecryptor(Key() As Byte, IV() As Byte) As System.Security.Cryptography.ICryptoTransform
            Dim aes As New System.Security.Cryptography.AesCryptoServiceProvider
            Return aes.CreateDecryptor(Key, IV)
        End Function
    End Class

    Public Interface iCompressionAssist
        Function CompressToBytes(ByVal Data As DataSet) As Byte()
        Function CompressToString(ByVal Data As DataSet) As String

        Function CompressToBytes(ByVal Data As IO.Stream) As Byte()
        Function CompressToString(ByVal Data As IO.Stream) As String

        Function CompressToBytes(ByVal Data() As Byte) As Byte()
        Function CompressToString(ByVal Data() As Byte) As String

        Function CompressToBytes(ByVal Data As String) As Byte()
        Function CompressToString(ByVal Data As String) As String

        Function DecompressToBytes(ByVal Compressed() As Byte) As Byte()
        Function DecompressToString(ByVal Compressed() As Byte) As String

        Function DecompressToBytes(ByVal Compressed As String) As Byte()
        Function DecompressToString(ByVal Compressed As String) As String

        Sub DecompressToStream(ByVal Compressed As String, ByVal Output As IO.Stream)
        Sub DecompressToStream(ByVal Compressed() As Byte, ByVal Output As IO.Stream)

        Sub DecompressToDataSet(ByVal Compressed As String, ByRef DS As DataSet)
        Sub DecompressToDataSet(ByVal Compressed() As Byte, ByRef DS As DataSet)
    End Interface

    Public Shared Function Salt(Length As Integer) As String
        If Length = 0 Then Return ""

        Randomize()
        Dim Key As Integer = Integer.Parse(Math.Abs(Rnd() * 123).ToString.Split(".")(0))
        Dim S As String = GetSeedKey(Key)
        If S.Length > Length Then
            Return Left(S, Length)
        ElseIf S = Length Then
            Return S
        Else
            Return S + Salt(Length - S.Length)
        End If
    End Function
    Public Shared Function StringSHA1(ByVal Data As String) As Byte()
        Using SHA As New System.Security.Cryptography.SHA1Managed
            Dim Buffer() As Byte = System.Text.Encoding.UTF8.GetBytes(Data)
            Try
                Dim Hash() As Byte = SHA.ComputeHash(Buffer)
                Erase Buffer
                Return Hash
            Catch ex As Exception
            Finally
                Erase Buffer
            End Try
        End Using
        Return Nothing
    End Function
    Public Shared Function StringSHA256(ByVal Data As String) As Byte()
        Using SHA As New System.Security.Cryptography.SHA256Managed
            'If Data Is Nothing Then Data = ""
            Dim Buffer() As Byte = System.Text.Encoding.UTF8.GetBytes(Data)
            Try
                Dim Hash() As Byte = SHA.ComputeHash(Buffer)
                Erase Buffer
                Return Hash
            Catch ex As Exception
            Finally
                Erase Buffer
            End Try
        End Using
        Return Nothing
    End Function
    Public Shared Function StringSHAVar(ByVal Data As String, NumBytes As Integer) As Byte()
        Try
            Dim Hash() As Byte = StringSHA256(Data)
            ReDim Preserve Hash(NumBytes - 1)
            Return Hash
        Catch ex As Exception
        Finally
        End Try
        Return Nothing
    End Function

    Public Shared Function GetSeedKey(Seed As Integer) As String
        Dim Result As String = ""
        For D As Double = 0 + Seed To (Math.PI * 2) + Seed Step 0.1965
            Dim R As String = (Math.Sin((D * Seed) / (D + Seed))).ToString("0.000000")
            If R.ToString() = "NaN" Then
                R = 10000
            End If
            Dim X As Integer = Integer.Parse(Microsoft.VisualBasic.Right(R, 4))
            Result += (Microsoft.VisualBasic.Chr(33 + (X Mod 92)))
        Next
        Return Result
    End Function
    ''' <summary>
    ''' Generates an encryption key based on Seed and encrypts the Data.
    ''' </summary>
    ''' <param name="Seed">Seed Index</param>
    ''' <param name="Data">String Data</param>
    ''' <returns>Base64 Encrypted Data</returns>
    ''' <remarks>Uses AES, No IV</remarks>
    Public Shared Function SeedEncrypt(Seed As Integer, Data As String, Optional ByVal SaltLength As Integer = 0) As String
        Return AES.EncryptString(Salt(SaltLength) + Data, GetSeedKey(Seed), "")
    End Function
    ''' <summary>
    ''' Generates an encryption key based on Seed and attempts to decrypt the Data.
    ''' </summary>
    ''' <param name="Seed">Seed Index</param>
    ''' <param name="Data">Base64 Encoded Encrypted Data</param>
    ''' <returns>Decrypted Data</returns>
    ''' <remarks>Uses AES, No IV</remarks>
    Public Shared Function SeedDecrypt(Seed As Integer, Data As String, Optional ByVal SaltLength As Integer = 0) As String
        Dim Result As String = AES.DecryptString(Data, GetSeedKey(Seed), "")
        Return Right(Result, Result.Length - SaltLength)
    End Function

    Public Shared Function TransformToString(Data As Byte(), CT As System.Security.Cryptography.ICryptoTransform) As String
        Dim bResult() As Byte = TransformToBytes(Data, CT)
        Dim sResult As String = Convert.ToBase64String(bResult)
        Erase bResult
        Return sResult
    End Function
    Public Shared Function TransformToString(Data As String, CT As System.Security.Cryptography.ICryptoTransform) As String
        Dim bResult() As Byte = TransformToBytes(Data, CT)
        Dim sResult As String = Convert.ToBase64String(bResult)
        Erase bResult
        Return sResult
    End Function
    Public Shared Function TransformToBytes(Data As Byte(), CT As System.Security.Cryptography.ICryptoTransform) As Byte()
        Try
            Return CT.TransformFinalBlock(Data, 0, Data.Length)
        Catch ex As Exception
            Throw New Exception("Error Transforming", ex)
            Return Nothing
        End Try
    End Function
    Public Shared Function TransformToBytes(Data As String, CT As System.Security.Cryptography.ICryptoTransform) As Byte()
        Dim bData() As Byte = Convert.FromBase64String(Data)
        Dim bResult() As Byte = TransformToBytes(bData, CT)
        Erase bData
        Return bResult
    End Function
End Class
