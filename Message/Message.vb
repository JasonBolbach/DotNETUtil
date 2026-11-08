Imports Assist
Imports System.Text
Imports System.Text.Json
Imports System.Security.Cryptography

''' <summary>
''' A tree of ID/Value nodes with two wire formats:
'''   Binary - gzip'd payload, DSA-signed, then AES-GCM encrypted (Serialize/DeSerialize with a Stream).
'''   Text   - JSON, optionally wrapped in a DSA-signed envelope (Serialize/DeSerialize with a String).
''' Deserialization never populates the target message unless the whole input was verified and parsed.
''' </summary>
Public Class Message
    ''' <summary>Maximum nesting depth accepted when deserializing.</summary>
    Public Const MaxDepth As Integer = 256
    ''' <summary>Maximum size in bytes of a single ID or Value accepted from the binary format.</summary>
    Public Const MaxStringBytes As Integer = 64 * 1024 * 1024
    ''' <summary>Maximum size in bytes of an encrypted binary message, and of its decompressed payload.</summary>
    Public Const MaxMessageBytes As Integer = 256 * 1024 * 1024

    Private Const NonceSize As Integer = 12
    Private Const TagSize As Integer = 16
    Private Const MaxSignatureBytes As Integer = 1024
    'Smallest possible serialized node: Int32 id length + Int32 value length + Int32 child count
    Private Const MinNodeBytes As Integer = 12

    'Header of the binary format ("BIM3"); also authenticated by GCM as associated data
    Private Shared ReadOnly Magic() As Byte = {&H42, &H49, &H4D, &H33}
    Private Shared ReadOnly pKey() As Byte = {&H0, &H1, &H2, &H3, &H4, &H5, &H6, &H7, &H8, &H9, &HA, &HB, &HC, &HD, &HE, &HF, &H10, &H11, &H12, &H13, &H14, &H15, &H16, &H17, &H18, &H19, &H1A, &H1B, &H1C, &H1D, &H1E, &H1F}
    Private Shared ReadOnly pDSA As New CA.DSA("PERTQUtleVZhbHVlPjxQPnNYdTV4V1R1M2k0cmwxTVVhZTVaWjkrdCt2UTZRaEdyTnc3ZEJJTmNnMURYOXUxYkpjZy9CVmx3N2tDV1JJQmtFR0NWM2cwSmFzVEpYRWdFSkpobGcvYzlEbThnckNxb0oxZ25BOE9NLzVTTCtIZ2hMQU9SbktRNEg0Y0tucmNHVGNrTGtaU3FJbzYyVGtLTFhJT2lpeE95NE1MS25MaXByZUlIR2M5UE1Waz08L1A+PFE+dk5CSkxJL0hURW50cVBsNUJKZTNmVzJRUmJrPTwvUT48Rz5MYXkvcnVXNW40bC9SaG1TNXUwV0h0bDdTak93RGREVVhuZUhrM3h2bEVUSzR0MUh6TjVjMEcwMERid3VXQkJHOWhLZTFHMWZDMTM1cWFyZGxGRExaN1BmRzM1UkJUNnZYMklDVFZKUUtpR0RueGpyS3orOE1yRGxDTFNydFRjb2l3aUFabEw4cXcxdEdPUjRrZ01DR1dmMGtBUC96RnFsZDh3WEFmTE0wMDg9PC9HPjxZPmgvQXRYaHNVMWVEV0xTNEpRQ1ZhR014UnRPZU5Zd281NXlLM1o2T3VTbVZXb25XUmpPRU5seHBvbHpGYTFsRjFsS21VOTRpK2s5bC9zcFVjaVM3aW0rOVovT3Zhd09yOU1BZTk2Yi9WdU5qWjB1eldzR0ZDTjJ5SjlTTy8zcFIvYWNSN0UvVWpOR2lHckxkM1UxWHFhMnJYUFpPeVJBdzE0a0VuUTV1VWs4RT08L1k+PFNlZWQ+L1dDbmVaTFczTDBXYWxhWDFIc2dJbnRPVW44PTwvU2VlZD48UGdlbkNvdW50ZXI+VFE9PTwvUGdlbkNvdW50ZXI+PFg+aHVIdzI3VWlNaVF4WnNuQmVWYXRZZW8wdWlvPTwvWD48L0RTQUtleVZhbHVlPg==")

    Private pID As String = ""
    Private pValue As String = ""
    Private pSubMessages As Dictionary(Of String, Message)

    Public Property Value() As String
        Get
            If pValue Is Nothing Then
                Return ""
            Else
                Return pValue
            End If
        End Get
        Set(ByVal value As String)
            pValue = value
        End Set
    End Property
    Public Property ID() As String
        Get
            Return pID
        End Get
        Set(ByVal value As String)
            pID = value
        End Set
    End Property

    Public Class BadMessageException
        Inherits Exception
        Public Sub New()
        End Sub
        Public Sub New(ByVal Message As String)
            MyBase.New(Message)
        End Sub
        Public Sub New(ByVal Message As String, ByVal InnerException As Exception)
            MyBase.New(Message, InnerException)
        End Sub
    End Class

    Public Function FindMessage(ByVal ID As String) As Message
        If pSubMessages Is Nothing Then Return Nothing
        Dim M As Message = Nothing
        pSubMessages.TryGetValue(ID, M)
        Return M
    End Function
    Public Sub AddMessage(ByVal NewMessage As Message)
        If pSubMessages Is Nothing Then pSubMessages = New Dictionary(Of String, Message)(StringComparer.InvariantCultureIgnoreCase)
        pSubMessages(NewMessage.ID) = NewMessage
    End Sub
    ''' <summary>Gets the child with the given ID, creating it if it does not exist. Use FindMessage to look up without creating.</summary>
    Default Public ReadOnly Property Messages(ByVal ID As String) As Message
        Get
            Dim M As Message = FindMessage(ID)
            If M Is Nothing Then
                M = New Message()
                M.ID = ID
                AddMessage(M)
            End If
            Return M
        End Get
    End Property

    ''' <summary>Copies the contents of a fully verified message into this one (children are merged by ID).</summary>
    Private Sub Adopt(Source As Message)
        Me.pID = Source.pID
        Me.pValue = Source.pValue
        If Source.pSubMessages IsNot Nothing Then
            For Each KVP As KeyValuePair(Of String, Message) In Source.pSubMessages
                AddMessage(KVP.Value)
            Next
        End If
    End Sub

#Region "Binary Mode Serialization"
    ''' <summary>
    ''' Writes: Magic(4) | Nonce(12) | Tag(16) | AES-GCM( SigLen(4) | Signature | GZip(Payload) ).
    ''' The signature is a DSA signature over SHA-256(Payload). Key must be 16, 24 or 32 bytes (Nothing = built-in key).
    ''' </summary>
    Public Overloads Sub Serialize(OutputStream As IO.Stream, Key() As Byte, DSAProvider As DSACryptoServiceProvider)
        If Key Is Nothing Then Key = pKey
        If DSAProvider Is Nothing Then DSAProvider = pDSA.DSA

        Dim Payload() As Byte
        Using PayloadStream As New IO.MemoryStream
            Using Writer As New IO.BinaryWriter(PayloadStream, Encoding.UTF8, True)
                Me.WriteTo(Writer)
            End Using
            Payload = PayloadStream.ToArray()
        End Using

        Dim Signature() As Byte = DSAProvider.SignData(SHA256.HashData(Payload))

        Dim PlainText() As Byte
        Using PlainStream As New IO.MemoryStream
            Using Writer As New IO.BinaryWriter(PlainStream, Encoding.UTF8, True)
                Writer.Write(Signature.Length)
                Writer.Write(Signature)
            End Using
            Using GZip As New IO.Compression.GZipStream(PlainStream, IO.Compression.CompressionLevel.Optimal, True)
                GZip.Write(Payload, 0, Payload.Length)
            End Using
            PlainText = PlainStream.ToArray()
        End Using

        Dim Nonce() As Byte = RandomNumberGenerator.GetBytes(NonceSize)
        Dim Tag(TagSize - 1) As Byte
        Dim CipherText(PlainText.Length - 1) As Byte
        Using GCM As New AesGcm(Key, TagSize)
            GCM.Encrypt(Nonce, PlainText, CipherText, Tag, Magic)
        End Using

        OutputStream.Write(Magic, 0, Magic.Length)
        OutputStream.Write(Nonce, 0, Nonce.Length)
        OutputStream.Write(Tag, 0, Tag.Length)
        OutputStream.Write(CipherText, 0, CipherText.Length)
    End Sub

    ''' <summary>
    ''' Reads a message written by Serialize. Returns True and populates this message only if the data
    ''' decrypts (GCM tag), decompresses, carries a valid DSA signature, and parses within the size/depth limits.
    ''' On any failure returns False and leaves this message unchanged.
    ''' </summary>
    Public Overloads Function DeSerialize(InputStream As IO.Stream, Key() As Byte, DSAProvider As DSACryptoServiceProvider) As Boolean
        If Key Is Nothing Then Key = pKey
        If DSAProvider Is Nothing Then DSAProvider = pDSA.DSA

        Try
            Dim Header(Magic.Length - 1) As Byte
            Dim Nonce(NonceSize - 1) As Byte
            Dim Tag(TagSize - 1) As Byte
            InputStream.ReadExactly(Header, 0, Header.Length)
            If Not Header.AsSpan().SequenceEqual(Magic) Then Return False
            InputStream.ReadExactly(Nonce, 0, Nonce.Length)
            InputStream.ReadExactly(Tag, 0, Tag.Length)

            Dim CipherText() As Byte = ReadLimited(InputStream)
            Dim PlainText(CipherText.Length - 1) As Byte
            Using GCM As New AesGcm(Key, TagSize)
                GCM.Decrypt(Nonce, CipherText, Tag, PlainText, Magic)
            End Using

            Dim Signature() As Byte
            Dim Payload() As Byte
            Using PlainStream As New IO.MemoryStream(PlainText)
                Using Reader As New IO.BinaryReader(PlainStream, Encoding.UTF8, True)
                    Dim SigLen As Integer = Reader.ReadInt32()
                    If SigLen <= 0 OrElse SigLen > MaxSignatureBytes OrElse SigLen > PlainStream.Length - PlainStream.Position Then Return False
                    Signature = Reader.ReadBytes(SigLen)
                End Using
                Using GZip As New IO.Compression.GZipStream(PlainStream, IO.Compression.CompressionMode.Decompress, True)
                    Payload = ReadLimited(GZip)
                End Using
            End Using

            If Not DSAProvider.VerifyData(SHA256.HashData(Payload), Signature) Then Return False

            Dim Parsed As New Message
            Using PayloadStream As New IO.MemoryStream(Payload)
                Using Reader As New IO.BinaryReader(PayloadStream, Encoding.UTF8, True)
                    Parsed.ReadFrom(Reader, 0)
                End Using
                If PayloadStream.Position <> PayloadStream.Length Then Return False
            End Using

            Adopt(Parsed)
            Return True
        Catch ex As Exception When TypeOf ex Is CryptographicException OrElse TypeOf ex Is IO.InvalidDataException OrElse TypeOf ex Is IO.EndOfStreamException OrElse TypeOf ex Is BadMessageException
            Return False
        End Try
    End Function

    Private Sub WriteTo(Writer As IO.BinaryWriter)
        WriteString(Writer, pID)
        WriteString(Writer, Value)
        Writer.Write(If(pSubMessages Is Nothing, 0, pSubMessages.Count))
        If pSubMessages IsNot Nothing Then
            For Each KVP As KeyValuePair(Of String, Message) In pSubMessages
                KVP.Value.WriteTo(Writer)
            Next
        End If
    End Sub
    Private Sub ReadFrom(Reader As IO.BinaryReader, Depth As Integer)
        If Depth > MaxDepth Then Throw New BadMessageException("Message nesting is too deep")

        Me.pID = ReadString(Reader)
        Me.pValue = ReadString(Reader)

        Dim Count As Integer = Reader.ReadInt32()
        Dim Remaining As Long = Reader.BaseStream.Length - Reader.BaseStream.Position
        If Count < 0 OrElse Count > Remaining \ MinNodeBytes Then Throw New BadMessageException("Invalid sub message count")

        For I As Integer = 0 To Count - 1
            Dim SM As New Message
            SM.ReadFrom(Reader, Depth + 1)
            Me.AddMessage(SM)
        Next
    End Sub
    Private Shared Sub WriteString(Writer As IO.BinaryWriter, S As String)
        Dim Bytes() As Byte = Encoding.UTF8.GetBytes(If(S, ""))
        Writer.Write(Bytes.Length)
        Writer.Write(Bytes)
    End Sub
    Private Shared Function ReadString(Reader As IO.BinaryReader) As String
        Dim Len As Integer = Reader.ReadInt32()
        Dim Remaining As Long = Reader.BaseStream.Length - Reader.BaseStream.Position
        If Len < 0 OrElse Len > MaxStringBytes OrElse Len > Remaining Then Throw New BadMessageException("Invalid string length")
        Return Encoding.UTF8.GetString(Reader.ReadBytes(Len))
    End Function
    ''' <summary>Reads a stream to its end, failing if it exceeds MaxMessageBytes.</summary>
    Private Shared Function ReadLimited(Input As IO.Stream) As Byte()
        Using Output As New IO.MemoryStream
            Dim Buffer(81919) As Byte
            Do
                Dim Count As Integer = Input.Read(Buffer, 0, Buffer.Length)
                If Count <= 0 Then Exit Do
                If Output.Length + Count > MaxMessageBytes Then Throw New BadMessageException("Message is too large")
                Output.Write(Buffer, 0, Count)
            Loop
            Return Output.ToArray()
        End Using
    End Function
#End Region

#Region "Text Mode Serialization (JSON)"
    'Unsigned form:  {"id":"...","value":"...","children":[ {...}, ... ]}
    'Signed form:    {"sig":"<base64 DSA signature of body>","body":"<the unsigned JSON, as a string>"}
    'The body is carried as a string so the signature is over exactly the bytes that were sent.

    ''' <summary>Returns this message tree as unsigned JSON.</summary>
    Public Function ToJson() As String
        Using MS As New IO.MemoryStream
            Using Writer As New Utf8JsonWriter(MS)
                WriteJson(Writer)
            End Using
            Return Encoding.UTF8.GetString(MS.ToArray())
        End Using
    End Function

    ''' <summary>Returns this message tree as a signed JSON envelope.</summary>
    Public Overloads Function Serialize(Optional Signer As CA.DSA = Nothing) As String
        If Signer Is Nothing Then Signer = pDSA

        Dim Body As String = ToJson()
        Using MS As New IO.MemoryStream
            Using Writer As New Utf8JsonWriter(MS)
                Writer.WriteStartObject()
                Writer.WriteString("sig", Signer.Sign(Body))
                Writer.WriteString("body", Body)
                Writer.WriteEndObject()
            End Using
            Return Encoding.UTF8.GetString(MS.ToArray())
        End Using
    End Function

    ''' <summary>Loads unsigned JSON produced by ToJson. Populates this message only if the whole input is valid.</summary>
    Public Overloads Sub DeSerialize(Input As String)
        Dim Parsed As New Message
        Try
            Using Doc As JsonDocument = JsonDocument.Parse(Input, JsonOptions)
                Parsed.ReadJson(Doc.RootElement, 0)
            End Using
        Catch ex As BadMessageException
            Throw
        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is KeyNotFoundException OrElse TypeOf ex Is ArgumentException
            Throw New BadMessageException("Malformed message", ex)
        End Try
        Adopt(Parsed)
    End Sub

    ''' <summary>Loads a signed envelope produced by Serialize. Throws BadMessageException if it is malformed or the signature is bad.</summary>
    Public Overloads Sub DeSerialize(Input As String, Signer As CA.DSA)
        Dim Body As String = Nothing
        If Not TryVerify(Input, Signer, Body) Then Throw New BadMessageException("Signature check failed or message is malformed")
        DeSerialize(Body)
    End Sub

    ''' <summary>True if Input is a well-formed signed envelope with a valid signature. Never throws.</summary>
    Public Shared Function Validate(Input As String, Signer As CA.DSA) As Boolean
        Dim Body As String = Nothing
        Return TryVerify(Input, Signer, Body)
    End Function

    Private Shared ReadOnly JsonOptions As New JsonDocumentOptions With {.MaxDepth = (MaxDepth * 2) + 8}

    Private Shared Function TryVerify(Input As String, Signer As CA.DSA, ByRef Body As String) As Boolean
        If Signer Is Nothing Then Signer = pDSA
        Try
            Using Doc As JsonDocument = JsonDocument.Parse(Input, JsonOptions)
                Dim Sig As String = Doc.RootElement.GetProperty("sig").GetString()
                Dim Candidate As String = Doc.RootElement.GetProperty("body").GetString()
                If Sig Is Nothing OrElse Candidate Is Nothing Then Return False
                If Not Signer.Verify(Candidate, Sig) Then Return False
                Body = Candidate
                Return True
            End Using
        Catch ex As Exception When TypeOf ex Is JsonException OrElse TypeOf ex Is InvalidOperationException OrElse TypeOf ex Is KeyNotFoundException OrElse TypeOf ex Is FormatException OrElse TypeOf ex Is CryptographicException
            Return False
        End Try
    End Function

    Private Sub WriteJson(Writer As Utf8JsonWriter)
        Writer.WriteStartObject()
        Writer.WriteString("id", pID)
        Writer.WriteString("value", Value)
        If pSubMessages IsNot Nothing AndAlso pSubMessages.Count > 0 Then
            Writer.WriteStartArray("children")
            For Each KVP As KeyValuePair(Of String, Message) In pSubMessages
                KVP.Value.WriteJson(Writer)
            Next
            Writer.WriteEndArray()
        End If
        Writer.WriteEndObject()
    End Sub
    Private Sub ReadJson(Element As JsonElement, Depth As Integer)
        If Depth > MaxDepth Then Throw New BadMessageException("Message nesting is too deep")
        If Element.ValueKind <> JsonValueKind.Object Then Throw New BadMessageException("Expected a JSON object")

        Me.pID = Element.GetProperty("id").GetString()
        Me.pValue = Element.GetProperty("value").GetString()

        Dim Children As JsonElement
        If Element.TryGetProperty("children", Children) Then
            For Each Child As JsonElement In Children.EnumerateArray()
                Dim SM As New Message
                SM.ReadJson(Child, Depth + 1)
                Me.AddMessage(SM)
            Next
        End If
    End Sub
#End Region

    Public Sub Clear()
        Me.pID = Nothing
        Me.pValue = Nothing
        If pSubMessages IsNot Nothing Then
            For Each KVP As KeyValuePair(Of String, Message) In pSubMessages
                KVP.Value.Clear()
            Next
            pSubMessages.Clear()
        End If
    End Sub

    Public Function Plot() As String
        Dim SB As New StringBuilder
        Me.Plot(SB, 1)
        Return SB.ToString()
    End Function
    Protected Sub Plot(ByRef SB As StringBuilder, Level As Integer)
        SB.Append(" "c, Level - 1)
        SB.Append("[" + ID + "]=[" + Value + "]" + vbCrLf)
        If pSubMessages IsNot Nothing Then
            For Each KVP As KeyValuePair(Of String, Message) In pSubMessages
                KVP.Value.Plot(SB, Level + 1)
            Next
        End If
    End Sub
End Class
