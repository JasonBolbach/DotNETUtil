Public Class IOTextBox
    Public Shadows Event TextChanged(sender As Object, e As System.EventArgs)

    Public Overloads Property Title As String
        Get
            Return Me.gbMain.Text
        End Get
        Set(value As String)
            Me.gbMain.Text = value
        End Set
    End Property

    Public Overloads Property Text As String
        Get
            Return Me.rtbMain.Text
        End Get
        Set(value As String)
            Me.rtbMain.Text = value
        End Set
    End Property
    Public ReadOnly Property CharCount As Integer
        Get
            Return Me.rtbMain.TextLength.ToString()
        End Get
    End Property
    Public ReadOnly Property ByteCount As Integer
        Get
            Return System.Text.UnicodeEncoding.Unicode.GetBytes(Me.rtbMain.Text).Length.ToString()
        End Get
    End Property
    Public ReadOnly Property Hash()
        Get
            Return Convert.ToBase64String(CryptoAssist.StringSHA1(Me.rtbMain.Text))
        End Get
    End Property

    Public Property Read_Only As Boolean
        Get
            Return Me.rtbMain.ReadOnly
        End Get
        Set(value As Boolean)
            Me.rtbMain.ReadOnly = value
        End Set
    End Property

    Private Sub rtbMain_TextChanged(sender As Object, e As EventArgs) Handles rtbMain.TextChanged
        Me.txtBytes.Text = ByteCount
        Me.txtChars.Text = CharCount
        Me.txtHash.Text = Hash
        RaiseEvent TextChanged(Me, New System.EventArgs)
    End Sub
End Class
