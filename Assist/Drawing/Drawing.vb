Public Class Drawing
    Public Enum RotationAnchor
        TopLeft
        CenterLeft
        BottomLeft

        CenterTop
        Center
        CenterBottom

        TopRight
        CenterRight
        BottomRight
    End Enum

    Public Shared Function GetRotationOffset(Size As System.Drawing.SizeF, Anchor As RotationAnchor) As System.Drawing.Point
        Return GetRotationOffset(Size.Width, Size.Height, Anchor)
    End Function
    Public Shared Function GetRotationOffset(Size As System.Drawing.Size, Anchor As RotationAnchor) As System.Drawing.Point
        Return GetRotationOffset(Size.Width, Size.Height, Anchor)
    End Function
    Public Shared Function GetRotationOffset(Width As Integer, Height As Integer, Anchor As RotationAnchor) As System.Drawing.Point
        Dim X, Y As Integer

        Select Case Anchor
            Case RotationAnchor.TopLeft
                X = 0 : Y = 0
            Case RotationAnchor.CenterLeft
                X = 0 : Y = Height / 2
            Case RotationAnchor.BottomLeft
                X = 0 : Y = Height

            Case RotationAnchor.CenterTop
                X = Width / 2 : Y = 0
            Case RotationAnchor.Center
                X = Width / 2 : Y = Height / 2
            Case RotationAnchor.CenterBottom
                X = Width / 2 : Y = Height

            Case RotationAnchor.TopRight
                X = Width : Y = 0
            Case RotationAnchor.CenterRight
                X = Width : Y = Height / 2
            Case RotationAnchor.BottomRight
                X = Width : Y = Height
        End Select

        Return New System.Drawing.Point(X, Y)
    End Function

    Public Shared Function SetupRotationTransform(G As System.Drawing.Graphics, Location As System.Drawing.Point, Angle As Integer) As System.Drawing.Drawing2D.GraphicsState
        Dim GS As System.Drawing.Drawing2D.GraphicsState = G.Save()
        G.RotateTransform(Angle)
        G.TranslateTransform(Location.X, Location.Y, System.Drawing.Drawing2D.MatrixOrder.Append)
        Return GS
    End Function

    Public Shared Sub DrawStringRotated(G As System.Drawing.Graphics, Text As String, Font As System.Drawing.Font, Brush As System.Drawing.Brush, Location As System.Drawing.Point, Angle As Integer, Anchor As RotationAnchor)
        Dim Offset As System.Drawing.Point = GetRotationOffset(G.MeasureString(Text, Font), Anchor)
        Dim GS As System.Drawing.Drawing2D.GraphicsState = SetupRotationTransform(G, Location, Angle)
        G.DrawString(Text, Font, Brush, -Offset.X, -Offset.Y)
        G.Restore(GS)
    End Sub
    Public Shared Sub DrawImageRotated(G As System.Drawing.Graphics, Image As System.Drawing.Image, Location As System.Drawing.Rectangle, Angle As Integer, Anchor As RotationAnchor)
        Dim ImgSize As New System.Drawing.SizeF(Location.Width, Location.Height)
        Dim Offset As System.Drawing.Point = GetRotationOffset(ImgSize, Anchor)
        Dim GS As System.Drawing.Drawing2D.GraphicsState = SetupRotationTransform(G, New System.Drawing.Point(Location.X, Location.Y), Angle)
        Dim ImgRect As New System.Drawing.Rectangle(-Offset.X, -Offset.Y, Location.Width, Location.Height)
        G.DrawImage(Image, ImgRect)
        G.Restore(GS)
    End Sub

    Public Shared Sub DrawRectangleRotated(G As System.Drawing.Graphics, RectPen As System.Drawing.Pen, Rect As System.Drawing.Rectangle, Angle As Integer, Anchor As RotationAnchor)
        Dim RectF As New System.Drawing.RectangleF(Rect.X, Rect.Y, Rect.Width, Rect.Height)
        Dim Offset As System.Drawing.Point = GetRotationOffset(RectF.Size, Anchor)
        Dim GS As System.Drawing.Drawing2D.GraphicsState = SetupRotationTransform(G, New System.Drawing.Point(Rect.X, Rect.Y), Angle)
        Dim ImgRect As New System.Drawing.Rectangle(-Offset.X, -Offset.Y, Rect.Width, Rect.Height)
        G.DrawRectangle(RectPen, ImgRect)
        G.Restore(GS)
    End Sub

End Class
