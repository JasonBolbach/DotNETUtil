<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class IOTextBox
    Inherits System.Windows.Forms.UserControl

    'UserControl overrides dispose to clean up the component list.
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
        Me.gbMain = New System.Windows.Forms.GroupBox()
        Me.lblChars = New System.Windows.Forms.Label()
        Me.txtChars = New System.Windows.Forms.TextBox()
        Me.lblBytes = New System.Windows.Forms.Label()
        Me.txtBytes = New System.Windows.Forms.TextBox()
        Me.rtbMain = New System.Windows.Forms.RichTextBox()
        Me.lblHash = New System.Windows.Forms.Label()
        Me.txtHash = New System.Windows.Forms.TextBox()
        Me.gbMain.SuspendLayout()
        Me.SuspendLayout()
        '
        'gbMain
        '
        Me.gbMain.Controls.Add(Me.lblHash)
        Me.gbMain.Controls.Add(Me.txtHash)
        Me.gbMain.Controls.Add(Me.lblChars)
        Me.gbMain.Controls.Add(Me.txtChars)
        Me.gbMain.Controls.Add(Me.lblBytes)
        Me.gbMain.Controls.Add(Me.txtBytes)
        Me.gbMain.Controls.Add(Me.rtbMain)
        Me.gbMain.Dock = System.Windows.Forms.DockStyle.Fill
        Me.gbMain.Location = New System.Drawing.Point(0, 0)
        Me.gbMain.Name = "gbMain"
        Me.gbMain.Size = New System.Drawing.Size(519, 208)
        Me.gbMain.TabIndex = 11
        Me.gbMain.TabStop = False
        '
        'lblChars
        '
        Me.lblChars.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblChars.AutoSize = True
        Me.lblChars.Location = New System.Drawing.Point(237, 179)
        Me.lblChars.Name = "lblChars"
        Me.lblChars.Size = New System.Drawing.Size(51, 20)
        Me.lblChars.TabIndex = 4
        Me.lblChars.Text = "Chars"
        '
        'txtChars
        '
        Me.txtChars.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtChars.Location = New System.Drawing.Point(294, 176)
        Me.txtChars.Name = "txtChars"
        Me.txtChars.ReadOnly = True
        Me.txtChars.Size = New System.Drawing.Size(77, 26)
        Me.txtChars.TabIndex = 3
        Me.txtChars.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'lblBytes
        '
        Me.lblBytes.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblBytes.AutoSize = True
        Me.lblBytes.Location = New System.Drawing.Point(377, 179)
        Me.lblBytes.Name = "lblBytes"
        Me.lblBytes.Size = New System.Drawing.Size(49, 20)
        Me.lblBytes.TabIndex = 2
        Me.lblBytes.Text = "Bytes"
        '
        'txtBytes
        '
        Me.txtBytes.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtBytes.Location = New System.Drawing.Point(432, 176)
        Me.txtBytes.Name = "txtBytes"
        Me.txtBytes.ReadOnly = True
        Me.txtBytes.Size = New System.Drawing.Size(81, 26)
        Me.txtBytes.TabIndex = 1
        Me.txtBytes.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'rtbMain
        '
        Me.rtbMain.Anchor = CType((((System.Windows.Forms.AnchorStyles.Top Or System.Windows.Forms.AnchorStyles.Bottom) _
            Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.rtbMain.Location = New System.Drawing.Point(6, 25)
        Me.rtbMain.Name = "rtbMain"
        Me.rtbMain.Size = New System.Drawing.Size(507, 145)
        Me.rtbMain.TabIndex = 0
        Me.rtbMain.Text = ""
        '
        'lblHash
        '
        Me.lblHash.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left), System.Windows.Forms.AnchorStyles)
        Me.lblHash.AutoSize = True
        Me.lblHash.Location = New System.Drawing.Point(6, 179)
        Me.lblHash.Name = "lblHash"
        Me.lblHash.Size = New System.Drawing.Size(47, 20)
        Me.lblHash.TabIndex = 6
        Me.lblHash.Text = "Hash"
        '
        'txtHash
        '
        Me.txtHash.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.txtHash.Location = New System.Drawing.Point(59, 176)
        Me.txtHash.Name = "txtHash"
        Me.txtHash.ReadOnly = True
        Me.txtHash.Size = New System.Drawing.Size(172, 26)
        Me.txtHash.TabIndex = 5
        Me.txtHash.TextAlign = System.Windows.Forms.HorizontalAlignment.Center
        '
        'IOTextBox
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(9.0!, 20.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.Controls.Add(Me.gbMain)
        Me.Name = "IOTextBox"
        Me.Size = New System.Drawing.Size(519, 208)
        Me.gbMain.ResumeLayout(False)
        Me.gbMain.PerformLayout()
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents gbMain As System.Windows.Forms.GroupBox
    Friend WithEvents lblChars As System.Windows.Forms.Label
    Friend WithEvents txtChars As System.Windows.Forms.TextBox
    Friend WithEvents lblBytes As System.Windows.Forms.Label
    Friend WithEvents txtBytes As System.Windows.Forms.TextBox
    Friend WithEvents rtbMain As System.Windows.Forms.RichTextBox
    Friend WithEvents lblHash As System.Windows.Forms.Label
    Friend WithEvents txtHash As System.Windows.Forms.TextBox

End Class
