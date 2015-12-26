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

    <Extension>
    Public Sub DrawRoundRectForm(ByVal thisform As Form, ByVal x As Single, ByVal y As Single, ByVal width As Single, ByVal height As Single, ByVal radius As Single)

        'draws a borderless form with curved edges
        Dim gp As GraphicsPath = New GraphicsPath()

        gp.AddLine(x + radius, y, x + width - (radius * 2), y)
        gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90)
        gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2))
        gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90)
        gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height)
        gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90)
        gp.AddLine(x, y + height - (radius * 2), x, y + radius)
        gp.AddArc(x, y, radius * 2, radius * 2, 180, 90)
        gp.CloseFigure()
        thisform.Region = New Region(gp)
        gp.Dispose()
    End Sub

    <Extension>
    Public Sub DrawRoundRectControl(ByVal thisControl As Control, ByVal x As Single, ByVal y As Single, ByVal width As Single, ByVal height As Single, ByVal radius As Single)

        'draws a control with curved edges
        Dim gp As GraphicsPath = New GraphicsPath()

        gp.AddLine(x + radius, y, x + width - (radius * 2), y)
        gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90)
        gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2))
        gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90)
        gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height)
        gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90)
        gp.AddLine(x, y + height - (radius * 2), x, y + radius)
        gp.AddArc(x, y, radius * 2, radius * 2, 180, 90)
        gp.CloseFigure()
        thisControl.Region = New Region(gp)
        gp.Dispose()
    End Sub

    <Extension>
    Public Sub DrawRoundRectFill(ByVal g As Graphics, ByVal b As Brush, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Single)

        ' fills object with color

        Dim gp As GraphicsPath = New GraphicsPath

        gp.AddLine(x + radius, y, x + width - (radius * 2), y)
        gp.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90)
        gp.AddLine(x + width, y + radius, x + width, y + height - (radius * 2))
        gp.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90)
        gp.AddLine(x + width - (radius * 2), y + height, x + radius, y + height)
        gp.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90)
        gp.AddLine(x, y + height - (radius * 2), x, y + radius)
        gp.AddArc(x, y, radius * 2, radius * 2, 180, 90)
        gp.CloseFigure()

        gp.CloseFigure()
        g.FillPath(b, gp)
        gp.Dispose()
    End Sub
End Module
