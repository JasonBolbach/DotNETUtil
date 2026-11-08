Public Class DemoForm

#Region "Compression"
    Private Sub InitCompressionTab()
        Using SR As New IO.StreamReader("Demo" + IO.Path.DirectorySeparatorChar + "LoremIpsum.txt")
            Me.iotbCompressInput.Text = SR.ReadToEnd
        End Using
        Me.cbCompressionMode.SelectedIndex = 0
    End Sub

    Private Sub cbCompressionMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbCompressionMode.SelectedIndexChanged
        UpdateCompression()
    End Sub
    Private Sub iotbCompressInput_TextChanged(sender As Object, e As EventArgs) Handles iotbCompressInput.TextChanged
        UpdateCompression()
    End Sub
    Private Sub cbEncMode_SelectedIndexChanged(sender As Object, e As EventArgs) Handles cbEncMode.SelectedIndexChanged
        UpdateEncryption()
    End Sub

    Private Sub UpdateCompression()
        If Me.cbCompressionMode.SelectedItem = "" Then Exit Sub

        Dim iC As iCompressionAssist = Nothing

        If Me.cbCompressionMode.SelectedItem = "GZip" Then iC = New GZipAssist
        If Me.cbCompressionMode.SelectedItem = "Deflate" Then iC = New DeflateAssist

        Me.iotbCompressOutput.Text = iC.CompressToString(iotbCompressInput.Text)

        Dim I As Double = iotbCompressInput.ByteCount
        Dim O As Double = iotbCompressOutput.ByteCount
        Me.txtCompressionRatio.Text = ((O / I) * 100).ToString("0.00")

        Me.iotbDecompressOutput.Text = iC.DecompressToString(iotbCompressOutput.Text)
    End Sub
#End Region

#Region "Encryption"
    Private Sub InitEncryption()
        Using SR As New IO.StreamReader("Demo" + IO.Path.DirectorySeparatorChar + "LoremIpsum.txt")
            Me.iotbEncIn.Text = SR.ReadToEnd
        End Using
        Me.txtEncKey.Text = Guid.NewGuid.ToString
        Me.cbEncMode.SelectedIndex = 0
    End Sub

    Private Sub iotbEncIn_TextChanged(sender As Object, e As EventArgs) Handles iotbEncIn.TextChanged
        UpdateEncryption()
    End Sub
    Private Sub txtEncKey_TextChanged(sender As Object, e As EventArgs) Handles txtEncKey.TextChanged
        UpdateEncryption()
    End Sub

    Private Sub UpdateEncryption()
        Select Case Me.cbEncMode.SelectedItem
            Case "AES"
                iotbEncOut.Text = AESAssist.EncryptString(iotbEncIn.Text, Me.txtEncKey.Text, "")
                iotbDecOut.Text = AESAssist.DecryptString(iotbEncOut.Text, Me.txtEncKey.Text, "")
            Case "DES"
                iotbEncOut.Text = DESAssist.EncryptString(iotbEncIn.Text, Me.txtEncKey.Text, "")
                iotbDecOut.Text = DESAssist.DecryptString(iotbEncOut.Text, Me.txtEncKey.Text, "")
            Case "TripleDES"
                iotbEncOut.Text = TripleDESAssist.EncryptString(iotbEncIn.Text, Me.txtEncKey.Text, "")
                iotbDecOut.Text = TripleDESAssist.DecryptString(iotbEncOut.Text, Me.txtEncKey.Text, "")
            Case Else
                Me.iotbEncOut.Text = ""
                Me.iotbDecOut.Text = ""
                Exit Sub
        End Select

    End Sub
#End Region

    Private Sub DemoForm_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        InitCompressionTab()
        InitEncryption()
        InitSeedEncryption()
    End Sub

    Private Sub tcMain_TabIndexChanged(sender As Object, e As EventArgs) Handles tcMain.SelectedIndexChanged
        If tcMain.SelectedTab Is tpEncryption Then iotbEncIn.Text = iotbEncIn.Text
        If tcMain.SelectedTab Is tpCompression Then iotbCompressInput.Text = iotbCompressInput.Text
        If tcMain.SelectedTab Is tpSeedEncrypt Then iotbSeedIn.Text = iotbSeedIn.Text
    End Sub

    Private Sub cmdExit_Click(sender As Object, e As EventArgs) Handles cmdExit.Click
        Me.Close()
    End Sub
#Region "Seed Encryption"
    Private Sub InitSeedEncryption()
        Me.txtSeed.Text = CryptoAssist.Salt(3)
        Me.iotbSeedIn.Text = "The quick brown fox jumped over the lazy dogs."
    End Sub

    Private Sub txtSeed_TextChanged(sender As Object, e As EventArgs) Handles txtSeed.TextChanged
        UpdateSeedEncryption()
    End Sub
    Private Sub txtSeedKey_TextChanged(sender As Object, e As EventArgs) Handles txtSeedKey.TextChanged
        UpdateSeedEncryption()
    End Sub
    Private Sub iotbSeedIn_Load(sender As Object, e As EventArgs) Handles iotbSeedIn.Load
        UpdateSeedEncryption()
    End Sub

    Private Sub UpdateSeedEncryption()
        Dim Seed As Integer = 0
        Try
            Seed = Integer.Parse(Me.txtSeed.Text)
        Catch ex As Exception
            Me.iotbSeedOut.Text = "BAD SEED!"
            Me.iotbSeedDec.Text = "BAD SEED!"
        End Try
        Me.txtSeedKey.Text = CryptoAssist.GetSeedKey(Seed)
        Me.iotbSeedOut.Text = CryptoAssist.SeedEncrypt(Seed, Me.iotbSeedIn.Text)
        Me.iotbSeedDec.Text = CryptoAssist.SeedDecrypt(Seed, Me.iotbSeedOut.Text)
    End Sub
#End Region

End Class