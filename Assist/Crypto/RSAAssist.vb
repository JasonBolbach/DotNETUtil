''' <summary>
''' Simplifies the usage of RSA encryption.
''' </summary>
''' <remarks></remarks>
Public Class RSAAssist
    Implements IDisposable

    Private pRSA As New System.Security.Cryptography.RSACryptoServiceProvider

    Public ReadOnly Property PublicOnly As Boolean
        Get
            Return pRSA.PublicOnly
        End Get
    End Property

    Public Function Sign(ByVal Data As String) As String
        Dim bData() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes(Data)
        Dim Signature As String = Sign(bData)
        Erase bData
        Return Signature
    End Function
    Public Function Sign(Data() As Byte)
        Dim bSignature() As Byte = pRSA.SignData(Data, New System.Security.Cryptography.SHA256Managed)
        Return Convert.ToBase64String(bSignature)
    End Function

    Public Function Verify(ByVal Data As String, ByVal Signature As String) As Boolean
        Dim bData() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes(Data)
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
        Dim XML As String = System.Text.ASCIIEncoding.ASCII.GetString(Bytes)
        pRSA.FromXmlString(XML)
    End Sub
    Public Function ExportKey(ByVal IncludePrivate As String) As String
        Dim XML As String = pRSA.ToXmlString(IncludePrivate)
        Dim Bytes() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes(XML)
        Dim Bytes64 As String = Convert.ToBase64String(Bytes)
        Return Bytes64
    End Function

    Public Function Encrypt(ByVal Data As String) As String
        Dim bData(), bEncrypted() As Byte
        Try
            bData = System.Text.ASCIIEncoding.ASCII.GetBytes(Data)
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
            Decrypted = System.Text.ASCIIEncoding.ASCII.GetString(bData)
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