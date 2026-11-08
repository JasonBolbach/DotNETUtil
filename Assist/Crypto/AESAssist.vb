''' <summary>
''' Simplifies usage of AES encryption libraries to Encrypt/Decrypt Strings with String based keys, and IVs
''' </summary>
''' <remarks></remarks>
Public Class AESAssist
    Public Shared Function EncryptString(Str As String, Key As String, IV As String) As String
        Return CryptoAssist.TransformToString(Text.Encoding.ASCII.GetBytes(Str), CreateEncryptor(Key, IV))
    End Function
    Public Shared Function DecryptString(Str As String, Key As String, IV As String) As String
        Return Text.Encoding.ASCII.GetString(CryptoAssist.TransformToBytes(Str, CreateDecryptor(Key, IV)))
    End Function
    Public Shared Function CreateEncryptor(Key As String, IV As String) As System.Security.Cryptography.ICryptoTransform
        Return CreateEncryptor(CryptoAssist.StringSHAVar(Key, 32), CryptoAssist.StringSHAVar(IV, 16))
    End Function
    Public Shared Function CreateEncryptor(Key() As Byte, IV() As Byte) As System.Security.Cryptography.ICryptoTransform
        Dim AES As New System.Security.Cryptography.AesCryptoServiceProvider
        Return AES.CreateEncryptor(Key, IV)
    End Function
    Public Shared Function CreateDecryptor(Key As String, IV As String) As System.Security.Cryptography.ICryptoTransform
        Return CreateDecryptor(CryptoAssist.StringSHAVar(Key, 32), CryptoAssist.StringSHAVar(IV, 16))
    End Function
    Public Shared Function CreateDecryptor(Key() As Byte, IV() As Byte) As System.Security.Cryptography.ICryptoTransform
        Dim aes As New System.Security.Cryptography.AesCryptoServiceProvider
        Return aes.CreateDecryptor(Key, IV)
    End Function
End Class