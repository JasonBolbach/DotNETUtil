''' <summary>
''' Base class for implementations of other Cryptography assist classes.
''' Implements SeedEncrypt/SeedDecrypt
''' </summary>
''' <remarks></remarks>
Public Class CryptoAssist
    Public Shared Function Salt(Length As Integer) As String
        Dim S As String = StrReverse(Math.Abs(Math.Sin(Now.Ticks * Rnd()) * 10000).ToString()).Split(".")(0)
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
            Dim Buffer() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes(Data)
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
            Dim Buffer() As Byte = System.Text.ASCIIEncoding.ASCII.GetBytes(Data)
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
    Public Shared Function SeedEncrypt(Seed As Integer, Data As String) As String
        Return AESAssist.EncryptString(Data, GetSeedKey(Seed), "")
    End Function
    ''' <summary>
    ''' Generates an encryption key based on Seed and attempts to decrypt the Data.
    ''' </summary>
    ''' <param name="Seed">Seed Index</param>
    ''' <param name="Data">Base64 Encoded Encrypted Data</param>
    ''' <returns>Decrypted Data</returns>
    ''' <remarks>Uses AES, No IV</remarks>
    Public Shared Function SeedDecrypt(Seed As Integer, Data As String) As String
        Return AESAssist.DecryptString(Data, GetSeedKey(Seed), "")
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