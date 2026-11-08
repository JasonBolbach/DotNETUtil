''' <summary>
''' Simplify Cross-Thread updates to Windows.Forms Controls
''' </summary>
Public Class InvokeAssist
    Private Delegate Sub SetTextBoxDelegate(ByVal C As Windows.Forms.TextBox, ByVal Text As String)
    Private Delegate Sub SetProgressBarDelegate(ByVal C As Windows.Forms.ProgressBar, ByVal Value As Integer)
    Private Delegate Sub SetRichTextBoxDelegate(ByVal C As Windows.Forms.RichTextBox, ByVal Value As String)
    Private Delegate Sub AppendRichTextBoxDelegate(ByVal C As Windows.Forms.RichTextBox, ByVal Value As String, ScrollToEnd As Boolean)
    Private Delegate Sub SetLabelDelegate(ByVal C As Windows.Forms.Label, ByVal Text As String)
    Private Delegate Sub SetCheckBoxDelegate(ByVal C As Windows.Forms.CheckBox, ByVal Checked As Boolean)
    Private Delegate Sub SetRadioButtonDelegate(ByVal C As Windows.Forms.RadioButton, ByVal Checked As Boolean)

    Public Shared Sub AppendRichTextBox(ByVal C As Windows.Forms.RichTextBox, ByVal Value As String, ScrollToEnd As Boolean)
        If C.InvokeRequired Then
            C.Invoke(New AppendRichTextBoxDelegate(AddressOf AppendRichTextBox), New Object() {C, Value, ScrollToEnd})
        Else
            C.Text += Value
            If ScrollToEnd Then
                C.Select(C.TextLength - 1, 0)
                C.ScrollToCaret()
            End If
        End If
    End Sub
    Public Shared Sub SetRichTextBox(ByVal C As Windows.Forms.RichTextBox, ByVal Value As String)
        If C.InvokeRequired Then
            C.Invoke(New SetRichTextBoxDelegate(AddressOf SetRichTextBox), New Object() {C, Value})
        Else
            C.Text = Value
        End If
    End Sub
    Public Shared Sub SetTextBox(ByVal C As Windows.Forms.TextBox, ByVal Value As String)
        If C.InvokeRequired Then
            C.Invoke(New SetTextBoxDelegate(AddressOf SetTextBox), New Object() {C, Value})
        Else
            C.Text = Value
        End If
    End Sub
    Public Shared Sub SetLabel(ByVal C As Windows.Forms.Label, ByVal Value As String)
        If C.InvokeRequired Then
            C.Invoke(New SetLabelDelegate(AddressOf SetLabel), New Object() {C, Value})
        Else
            C.Text = Value
        End If
    End Sub
    Public Shared Sub SetCheckBox(ByVal C As Windows.Forms.CheckBox, ByVal Value As Boolean)
        If C.InvokeRequired Then
            C.Invoke(New SetCheckBoxDelegate(AddressOf SetCheckBox), New Object() {C, Value})
        Else
            C.Text = Value
        End If
    End Sub
    Public Shared Sub SetRadioButton(ByVal C As Windows.Forms.RadioButton, ByVal Value As Boolean)
        If C.InvokeRequired Then
            C.Invoke(New SetRadioButtonDelegate(AddressOf SetRadioButton), New Object() {C, Value})
        Else
            C.Text = Value
        End If
    End Sub
    Public Shared Sub SetProgressBar(ByVal C As Windows.Forms.ProgressBar, ByVal Value As Integer)
        If C.InvokeRequired Then
            C.Invoke(New SetProgressBarDelegate(AddressOf SetProgressBar), New Object() {C, Value})
        Else
            C.Value = Value
        End If
    End Sub
End Class
