Imports System.Drawing.Drawing2D

Public Class FrmInput

    Private Sub FrmInput_Load(sender As System.Object, e As System.EventArgs) Handles MyBase.Load

    End Sub

    Private Sub FrmInput_Paint(sender As Object, e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint

        On Error Resume Next
        If sender.width = 0 Then Exit Sub
        If sender.height = 0 Then Exit Sub

        Dim a As New LinearGradientBrush(New RectangleF(0, 0, Me.Width, Me.Height), Color.SlateGray, Color.Black, LinearGradientMode.Vertical)
        Dim gg As Graphics = e.Graphics
        gg.FillRectangle(a, New RectangleF(0, 0, Me.Width, Me.Height))
        a = Nothing
        gg = Nothing

    End Sub
End Class