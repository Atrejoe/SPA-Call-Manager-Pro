Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices

Module UiUtils
    <Extension>
    Public Sub PaintGradient(control As Control, graphics As Graphics, optional color1 As Color?=Nothing, optional color2 As Color? = Nothing)
        Try
            If control.Width = 0 Then Exit Sub
            If control.Height = 0 Then Exit Sub

            Dim a As New LinearGradientBrush(New RectangleF(0, 0, control.Width, control.Height), color1.GetValueOrDefault(Color.SlateGray), color2.GetValueOrDefault(Color.Black), LinearGradientMode.Vertical)
            Dim gg As Graphics = graphics
            gg.FillRectangle(a, New RectangleF(0, 0, control.Width, control.Height))

        Catch ex As Exception
            ex.Log()
        End Try
    End Sub
End Module
