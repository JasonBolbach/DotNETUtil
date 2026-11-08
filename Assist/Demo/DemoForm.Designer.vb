<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class DemoForm
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.cmdExit = New System.Windows.Forms.Button()
        Me.tpEncryption = New System.Windows.Forms.TabPage()
        Me.gbEncKEy = New System.Windows.Forms.GroupBox()
        Me.txtEncKey = New System.Windows.Forms.TextBox()
        Me.iotbDecOut = New Assist.IOTextBox()
        Me.iotbEncOut = New Assist.IOTextBox()
        Me.iotbEncIn = New Assist.IOTextBox()
        Me.gbEncMode = New System.Windows.Forms.GroupBox()
        Me.cbEncMode = New System.Windows.Forms.ComboBox()
        Me.tpCompression = New System.Windows.Forms.TabPage()
        Me.iotbDecompressOutput = New Assist.IOTextBox()
        Me.iotbCompressOutput = New Assist.IOTextBox()
        Me.iotbCompressInput = New Assist.IOTextBox()
        Me.gbCompressionRatio = New System.Windows.Forms.GroupBox()
        Me.txtCompressionRatio = New System.Windows.Forms.TextBox()
        Me.gbCompressionMode = New System.Windows.Forms.GroupBox()
        Me.cbCompressionMode = New System.Windows.Forms.ComboBox()
        Me.tcMain = New System.Windows.Forms.TabControl()
        Me.tpSeedEncrypt = New System.Windows.Forms.TabPage()
        Me.gbSeed = New System.Windows.Forms.GroupBox()
        Me.txtSeed = New System.Windows.Forms.TextBox()
        Me.iotbSeedDec = New Assist.IOTextBox()
        Me.iotbSeedOut = New Assist.IOTextBox()
        Me.iotbSeedIn = New Assist.IOTextBox()
        Me.gbSeedKey = New System.Windows.Forms.GroupBox()
        Me.txtSeedKey = New System.Windows.Forms.TextBox()
        Me.tpEncryption.SuspendLayout()
        Me.gbEncKEy.SuspendLayout()
        Me.gbEncMode.SuspendLayout()
        Me.tpCompression.SuspendLayout()
        Me.gbCompressionRatio.SuspendLayout()
        Me.gbCompressionMode.SuspendLayout()
        Me.tcMain.SuspendLayout()
        Me.tpSeedEncrypt.SuspendLayout()
        Me.gbSeed.SuspendLayout()
        Me.gbSeedKey.SuspendLayout()
        Me.SuspendLayout()
        '
        'cmdExit
        '
        Me.cmdExit.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cmdExit.Location = New System.Drawing.Point(579, 791)
        Me.cmdExit.Name = "cmdExit"
        Me.cmdExit.Size = New System.Drawing.Size(100, 39)
        Me.cmdExit.TabIndex = 1
        Me.cmdExit.Text = "Exit"
        Me.cmdExit.UseVisualStyleBackColor = True
        '
        'tpEncryption
        '
        Me.tpEncryption.Controls.Add(Me.gbEncKEy)
        Me.tpEncryption.Controls.Add(Me.iotbDecOut)
        Me.tpEncryption.Controls.Add(Me.iotbEncOut)
        Me.tpEncryption.Controls.Add(Me.iotbEncIn)
        Me.tpEncryption.Controls.Add(Me.gbEncMode)
        Me.tpEncryption.Location = New System.Drawing.Point(4, 29)
        Me.tpEncryption.Name = "tpEncryption"
        Me.tpEncryption.Padding = New System.Windows.Forms.Padding(3)
        Me.tpEncryption.Size = New System.Drawing.Size(663, 730)
        Me.tpEncryption.TabIndex = 1
        Me.tpEncryption.Text = "Encryption"
        Me.tpEncryption.UseVisualStyleBackColor = True
        '
        'gbEncKEy
        '
        Me.gbEncKEy.Controls.Add(Me.txtEncKey)
        Me.gbEncKEy.Location = New System.Drawing.Point(287, 6)
        Me.gbEncKEy.Name = "gbEncKEy"
        Me.gbEncKEy.Size = New System.Drawing.Size(370, 70)
        Me.gbEncKEy.TabIndex = 22
        Me.gbEncKEy.TabStop = False
        Me.gbEncKEy.Text = "Encryption Key"
        '
        'txtEncKey
        '
        Me.txtEncKey.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtEncKey.Location = New System.Drawing.Point(6, 27)
        Me.txtEncKey.Name = "txtEncKey"
        Me.txtEncKey.Size = New System.Drawing.Size(358, 26)
        Me.txtEncKey.TabIndex = 5
        Me.txtEncKey.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'iotbDecOut
        '
        Me.iotbDecOut.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.iotbDecOut.Location = New System.Drawing.Point(3, 508)
        Me.iotbDecOut.Name = "iotbDecOut"
        Me.iotbDecOut.Read_Only = True
        Me.iotbDecOut.Size = New System.Drawing.Size(651, 207)
        Me.iotbDecOut.TabIndex = 21
        Me.iotbDecOut.Title = "Decrypted"
        '
        'iotbEncOut
        '
        Me.iotbEncOut.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.iotbEncOut.Location = New System.Drawing.Point(6, 295)
        Me.iotbEncOut.Name = "iotbEncOut"
        Me.iotbEncOut.Read_Only = True
        Me.iotbEncOut.Size = New System.Drawing.Size(651, 207)
        Me.iotbEncOut.TabIndex = 20
        Me.iotbEncOut.Title = "Encrypted (Base64)"
        '
        'iotbEncIn
        '
        Me.iotbEncIn.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.iotbEncIn.Location = New System.Drawing.Point(6, 82)
        Me.iotbEncIn.Name = "iotbEncIn"
        Me.iotbEncIn.Read_Only = False
        Me.iotbEncIn.Size = New System.Drawing.Size(651, 207)
        Me.iotbEncIn.TabIndex = 19
        Me.iotbEncIn.Title = "Input"
        '
        'gbEncMode
        '
        Me.gbEncMode.Controls.Add(Me.cbEncMode)
        Me.gbEncMode.Location = New System.Drawing.Point(6, 6)
        Me.gbEncMode.Name = "gbEncMode"
        Me.gbEncMode.Size = New System.Drawing.Size(275, 70)
        Me.gbEncMode.TabIndex = 18
        Me.gbEncMode.TabStop = False
        Me.gbEncMode.Text = "Encryption Mode"
        '
        'cbEncMode
        '
        Me.cbEncMode.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cbEncMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbEncMode.FormattingEnabled = True
        Me.cbEncMode.Items.AddRange(New Object() {"AES", "DES", "TripleDES"})
        Me.cbEncMode.Location = New System.Drawing.Point(6, 25)
        Me.cbEncMode.Name = "cbEncMode"
        Me.cbEncMode.Size = New System.Drawing.Size(263, 28)
        Me.cbEncMode.TabIndex = 0
        '
        'tpCompression
        '
        Me.tpCompression.Controls.Add(Me.iotbDecompressOutput)
        Me.tpCompression.Controls.Add(Me.iotbCompressOutput)
        Me.tpCompression.Controls.Add(Me.iotbCompressInput)
        Me.tpCompression.Controls.Add(Me.gbCompressionRatio)
        Me.tpCompression.Controls.Add(Me.gbCompressionMode)
        Me.tpCompression.Location = New System.Drawing.Point(4, 29)
        Me.tpCompression.Name = "tpCompression"
        Me.tpCompression.Padding = New System.Windows.Forms.Padding(3)
        Me.tpCompression.Size = New System.Drawing.Size(663, 730)
        Me.tpCompression.TabIndex = 0
        Me.tpCompression.Text = "Compression"
        Me.tpCompression.UseVisualStyleBackColor = True
        '
        'iotbDecompressOutput
        '
        Me.iotbDecompressOutput.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.iotbDecompressOutput.Location = New System.Drawing.Point(6, 508)
        Me.iotbDecompressOutput.Name = "iotbDecompressOutput"
        Me.iotbDecompressOutput.Read_Only = True
        Me.iotbDecompressOutput.Size = New System.Drawing.Size(651, 207)
        Me.iotbDecompressOutput.TabIndex = 17
        Me.iotbDecompressOutput.Title = "Decompressed"
        '
        'iotbCompressOutput
        '
        Me.iotbCompressOutput.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.iotbCompressOutput.Location = New System.Drawing.Point(6, 295)
        Me.iotbCompressOutput.Name = "iotbCompressOutput"
        Me.iotbCompressOutput.Read_Only = True
        Me.iotbCompressOutput.Size = New System.Drawing.Size(651, 207)
        Me.iotbCompressOutput.TabIndex = 16
        Me.iotbCompressOutput.Title = "Compressed (Base64)"
        '
        'iotbCompressInput
        '
        Me.iotbCompressInput.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.iotbCompressInput.Location = New System.Drawing.Point(6, 82)
        Me.iotbCompressInput.Name = "iotbCompressInput"
        Me.iotbCompressInput.Read_Only = False
        Me.iotbCompressInput.Size = New System.Drawing.Size(651, 207)
        Me.iotbCompressInput.TabIndex = 15
        Me.iotbCompressInput.Title = "Input"
        '
        'gbCompressionRatio
        '
        Me.gbCompressionRatio.Controls.Add(Me.txtCompressionRatio)
        Me.gbCompressionRatio.Location = New System.Drawing.Point(287, 6)
        Me.gbCompressionRatio.Name = "gbCompressionRatio"
        Me.gbCompressionRatio.Size = New System.Drawing.Size(370, 70)
        Me.gbCompressionRatio.TabIndex = 14
        Me.gbCompressionRatio.TabStop = False
        Me.gbCompressionRatio.Text = "Compression Ratio"
        '
        'txtCompressionRatio
        '
        Me.txtCompressionRatio.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtCompressionRatio.Location = New System.Drawing.Point(6, 27)
        Me.txtCompressionRatio.Name = "txtCompressionRatio"
        Me.txtCompressionRatio.ReadOnly = True
        Me.txtCompressionRatio.Size = New System.Drawing.Size(358, 26)
        Me.txtCompressionRatio.TabIndex = 5
        Me.txtCompressionRatio.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'gbCompressionMode
        '
        Me.gbCompressionMode.Controls.Add(Me.cbCompressionMode)
        Me.gbCompressionMode.Location = New System.Drawing.Point(6, 6)
        Me.gbCompressionMode.Name = "gbCompressionMode"
        Me.gbCompressionMode.Size = New System.Drawing.Size(275, 70)
        Me.gbCompressionMode.TabIndex = 13
        Me.gbCompressionMode.TabStop = False
        Me.gbCompressionMode.Text = "Compression Mode"
        '
        'cbCompressionMode
        '
        Me.cbCompressionMode.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.cbCompressionMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList
        Me.cbCompressionMode.FormattingEnabled = True
        Me.cbCompressionMode.Items.AddRange(New Object() {"GZip", "Deflate"})
        Me.cbCompressionMode.Location = New System.Drawing.Point(6, 25)
        Me.cbCompressionMode.Name = "cbCompressionMode"
        Me.cbCompressionMode.Size = New System.Drawing.Size(263, 28)
        Me.cbCompressionMode.TabIndex = 0
        '
        'tcMain
        '
        Me.tcMain.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.tcMain.Controls.Add(Me.tpCompression)
        Me.tcMain.Controls.Add(Me.tpEncryption)
        Me.tcMain.Controls.Add(Me.tpSeedEncrypt)
        Me.tcMain.Location = New System.Drawing.Point(12, 12)
        Me.tcMain.Name = "tcMain"
        Me.tcMain.SelectedIndex = 0
        Me.tcMain.Size = New System.Drawing.Size(671, 763)
        Me.tcMain.TabIndex = 0
        '
        'tpSeedEncrypt
        '
        Me.tpSeedEncrypt.Controls.Add(Me.gbSeedKey)
        Me.tpSeedEncrypt.Controls.Add(Me.gbSeed)
        Me.tpSeedEncrypt.Controls.Add(Me.iotbSeedDec)
        Me.tpSeedEncrypt.Controls.Add(Me.iotbSeedOut)
        Me.tpSeedEncrypt.Controls.Add(Me.iotbSeedIn)
        Me.tpSeedEncrypt.Location = New System.Drawing.Point(4, 29)
        Me.tpSeedEncrypt.Name = "tpSeedEncrypt"
        Me.tpSeedEncrypt.Padding = New System.Windows.Forms.Padding(3)
        Me.tpSeedEncrypt.Size = New System.Drawing.Size(663, 730)
        Me.tpSeedEncrypt.TabIndex = 2
        Me.tpSeedEncrypt.Text = "Seed Encrypt"
        Me.tpSeedEncrypt.UseVisualStyleBackColor = True
        '
        'gbSeed
        '
        Me.gbSeed.Controls.Add(Me.txtSeed)
        Me.gbSeed.Location = New System.Drawing.Point(6, 6)
        Me.gbSeed.Name = "gbSeed"
        Me.gbSeed.Size = New System.Drawing.Size(167, 70)
        Me.gbSeed.TabIndex = 26
        Me.gbSeed.TabStop = False
        Me.gbSeed.Text = "Seed (Integer)"
        '
        'txtSeed
        '
        Me.txtSeed.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSeed.Location = New System.Drawing.Point(6, 27)
        Me.txtSeed.Name = "txtSeed"
        Me.txtSeed.Size = New System.Drawing.Size(155, 26)
        Me.txtSeed.TabIndex = 5
        Me.txtSeed.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'iotbSeedDec
        '
        Me.iotbSeedDec.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.iotbSeedDec.Location = New System.Drawing.Point(6, 508)
        Me.iotbSeedDec.Name = "iotbSeedDec"
        Me.iotbSeedDec.Read_Only = True
        Me.iotbSeedDec.Size = New System.Drawing.Size(651, 207)
        Me.iotbSeedDec.TabIndex = 25
        Me.iotbSeedDec.Title = "Decrypted"
        '
        'iotbSeedOut
        '
        Me.iotbSeedOut.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.iotbSeedOut.Location = New System.Drawing.Point(6, 295)
        Me.iotbSeedOut.Name = "iotbSeedOut"
        Me.iotbSeedOut.Read_Only = True
        Me.iotbSeedOut.Size = New System.Drawing.Size(651, 207)
        Me.iotbSeedOut.TabIndex = 24
        Me.iotbSeedOut.Title = "Encrypted (Base64)"
        '
        'iotbSeedIn
        '
        Me.iotbSeedIn.Anchor = CType(((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.iotbSeedIn.Location = New System.Drawing.Point(6, 82)
        Me.iotbSeedIn.Name = "iotbSeedIn"
        Me.iotbSeedIn.Read_Only = False
        Me.iotbSeedIn.Size = New System.Drawing.Size(651, 207)
        Me.iotbSeedIn.TabIndex = 23
        Me.iotbSeedIn.Title = "Input"
        '
        'gbSeedKey
        '
        Me.gbSeedKey.Controls.Add(Me.txtSeedKey)
        Me.gbSeedKey.Location = New System.Drawing.Point(179, 6)
        Me.gbSeedKey.Name = "gbSeedKey"
        Me.gbSeedKey.Size = New System.Drawing.Size(478, 70)
        Me.gbSeedKey.TabIndex = 27
        Me.gbSeedKey.TabStop = False
        Me.gbSeedKey.Text = "Seed Key"
        '
        'txtSeedKey
        '
        Me.txtSeedKey.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtSeedKey.Location = New System.Drawing.Point(6, 27)
        Me.txtSeedKey.Name = "txtSeedKey"
        Me.txtSeedKey.Size = New System.Drawing.Size(466, 26)
        Me.txtSeedKey.TabIndex = 5
        Me.txtSeedKey.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'DemoForm
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(695, 842)
        Me.Controls.Add(Me.cmdExit)
        Me.Controls.Add(Me.tcMain)
        Me.Name = "DemoForm"
        Me.Text = "DemoForm"
        Me.tpEncryption.ResumeLayout(False)
        Me.gbEncKEy.ResumeLayout(False)
        Me.gbEncKEy.PerformLayout()
        Me.gbEncMode.ResumeLayout(False)
        Me.tpCompression.ResumeLayout(False)
        Me.gbCompressionRatio.ResumeLayout(False)
        Me.gbCompressionRatio.PerformLayout()
        Me.gbCompressionMode.ResumeLayout(False)
        Me.tcMain.ResumeLayout(False)
        Me.tpSeedEncrypt.ResumeLayout(False)
        Me.gbSeed.ResumeLayout(False)
        Me.gbSeed.PerformLayout()
        Me.gbSeedKey.ResumeLayout(False)
        Me.gbSeedKey.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents cmdExit As System.Windows.Forms.Button
    Friend WithEvents tpEncryption As System.Windows.Forms.TabPage
    Friend WithEvents tpCompression As System.Windows.Forms.TabPage
    Friend WithEvents iotbDecompressOutput As Assist.IOTextBox
    Friend WithEvents iotbCompressOutput As Assist.IOTextBox
    Friend WithEvents iotbCompressInput As Assist.IOTextBox
    Friend WithEvents gbCompressionRatio As System.Windows.Forms.GroupBox
    Friend WithEvents txtCompressionRatio As System.Windows.Forms.TextBox
    Friend WithEvents gbCompressionMode As System.Windows.Forms.GroupBox
    Friend WithEvents cbCompressionMode As System.Windows.Forms.ComboBox
    Friend WithEvents tcMain As System.Windows.Forms.TabControl
    Friend WithEvents gbEncKEy As System.Windows.Forms.GroupBox
    Friend WithEvents txtEncKey As System.Windows.Forms.TextBox
    Friend WithEvents iotbDecOut As Assist.IOTextBox
    Friend WithEvents iotbEncOut As Assist.IOTextBox
    Friend WithEvents iotbEncIn As Assist.IOTextBox
    Friend WithEvents gbEncMode As System.Windows.Forms.GroupBox
    Friend WithEvents cbEncMode As System.Windows.Forms.ComboBox
    Friend WithEvents tpSeedEncrypt As System.Windows.Forms.TabPage
    Friend WithEvents gbSeedKey As System.Windows.Forms.GroupBox
    Friend WithEvents txtSeedKey As System.Windows.Forms.TextBox
    Friend WithEvents gbSeed As System.Windows.Forms.GroupBox
    Friend WithEvents txtSeed As System.Windows.Forms.TextBox
    Friend WithEvents iotbSeedDec As Assist.IOTextBox
    Friend WithEvents iotbSeedOut As Assist.IOTextBox
    Friend WithEvents iotbSeedIn As Assist.IOTextBox
End Class
