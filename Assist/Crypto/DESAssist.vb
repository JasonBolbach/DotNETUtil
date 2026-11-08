''' <summary>
''' Simplifies usage of DES encryption libraries to Encrypt/Decrypt Strings with String based keys, and IVs
''' </summary>
''' <remarks></remarks>
Public Class DESAssist
    Private Shared Function StringSHA256(ByVal Data As String) As Byte()
        Using SHA256 As New System.Security.Cryptography.SHA256Managed
            Dim Buffer() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes(Data)
            Try
                Dim Hash() As Byte = SHA256.ComputeHash(Buffer)
                Erase Buffer
                Return Hash
            Catch ex As Exception
            Finally
                Erase Buffer
            End Try
        End Using
        Return Nothing
    End Function
    Private Shared Function StringSHAVar(ByVal Data As String, NumBytes As Integer) As Byte()
        Using SHA256 As New System.Security.Cryptography.SHA256Managed
            Dim Buffer() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes(Data)
            Try
                Dim Hash() As Byte = SHA256.ComputeHash(Buffer)
                ReDim Preserve Hash(NumBytes - 1)
                Erase Buffer
                Return Hash
            Catch ex As Exception
            Finally
                Erase Buffer
            End Try
        End Using
        Return Nothing
    End Function
    Public Shared Function EncryptString(Str As String, Key As String, IV As String) As String
        Return CryptoAssist.TransformToString(Text.Encoding.ASCII.GetBytes(Str), CreateEncryptor(Key, IV))
    End Function
    Public Shared Function DecryptString(Str As String, Key As String, IV As String) As String
        Return Text.Encoding.ASCII.GetString(CryptoAssist.TransformToBytes(Str, CreateDecryptor(Key, IV)))
    End Function

    Public Shared Function CreateEncryptor(Key As String, IV As String) As System.Security.Cryptography.ICryptoTransform
        Return CreateEncryptor(StringSHAVar(Key, 8), StringSHAVar(IV, 8))
    End Function
    Public Shared Function CreateEncryptor(Key() As Byte, IV() As Byte) As System.Security.Cryptography.ICryptoTransform
        Dim DES As New System.Security.Cryptography.DESCryptoServiceProvider
        Return DES.CreateEncryptor(Key, IV)
    End Function
    Public Shared Function CreateDecryptor(Key As String, IV As String) As System.Security.Cryptography.ICryptoTransform
        Return CreateDecryptor(StringSHAVar(Key, 8), StringSHAVar(IV, 8))
    End Function
    Public Shared Function CreateDecryptor(Key() As Byte, IV() As Byte) As System.Security.Cryptography.ICryptoTransform
        Dim DES As New System.Security.Cryptography.DESCryptoServiceProvider
        Return DES.CreateDecryptor(Key, IV)
    End Function
End Class