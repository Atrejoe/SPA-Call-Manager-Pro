Imports System.Drawing.Drawing2D

Public Class FrmCall

    ReadOnly _callBorderColor As Color = Color.Black
    Dim _frmCallPhoneStatus As SPhoneStatus

    Private Sub FrmCall_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim g As Graphics = Me.CreateGraphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        DrawRoundRectForm(0, 0, Me.Width, Me.Height, 15)
        Me.BackColor = _callBorderColor

    End Sub

    Private Sub FrmCall_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint

        Dim g As Graphics = e.Graphics

        Dim textFont As New Font("trebuchet MS", 12, FontStyle.Bold)

        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        Dim fillBrush As New LinearGradientBrush(New RectangleF(0, 0, Me.Width, Me.Height), Color.White, Color.Gray, LinearGradientMode.Vertical)
        g.DrawRoundRectFill(fillBrush, 2, 2, Me.Width - 4, Me.Height - 4, 15)

        g.DrawString("Incoming Call Line: " & _frmCallPhoneStatus.Id, textFont, Brushes.Black, 10, 10)

        If _frmCallPhoneStatus.CallerName = "" Then
            g.DrawString(_frmCallPhoneStatus.CallerNumber, textFont, Brushes.Black, 10, 30)
        Else
            g.DrawString(_frmCallPhoneStatus.CallerName, textFont, Brushes.Black, 10, 30)
        End If

    End Sub

    Private Sub BtnAnswer_Click(sender As Object, e As EventArgs) Handles BtnAnswer.Click

        Dim callString As String = PhoneAction(eAction.Answer, _frmCallPhoneStatus, MyPhoneSettings)

        SendUdp(callString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
    End Sub

    Private Sub BtnAnswer_MouseEnter(sender As Object, e As EventArgs) Handles BtnAnswer.MouseEnter

        Me.Cursor = Cursors.Hand

    End Sub

    Private Sub BtnAnswer_MouseLeave(sender As Object, e As EventArgs) Handles BtnAnswer.MouseLeave

        Me.Cursor = Cursors.Default

    End Sub

    Private Sub BtnAnswer_Paint(sender As Object, e As PaintEventArgs) Handles BtnAnswer.Paint

        Dim g As Graphics = e.Graphics
        Dim callImage As New Bitmap(My.Resources.Phone1, 60, 60)

        BtnAnswer.DrawRoundRectControl(0, 0, Me.Width, Me.Height, 5)
        BtnAnswer.PaintGradient(e.Graphics, Color.Green, Color.DarkGreen)
        g.DrawImage(callImage, 15, 0)


    End Sub

    Private Sub BtnReject_Click(sender As Object, e As EventArgs) Handles BtnReject.Click

        Dim callString As String = PhoneAction(eAction.Reject, _frmCallPhoneStatus, MyPhoneSettings)

        SendUdp(callString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
    End Sub

    Private Sub BtnReject_MouseEnter(sender As Object, e As EventArgs) Handles BtnReject.MouseEnter

        Me.Cursor = Cursors.Hand

    End Sub

    Private Sub BtnReject_MouseLeave(sender As Object, e As EventArgs) Handles BtnReject.MouseLeave

        Me.Cursor = Cursors.Default

    End Sub

    Private Sub BtnReject_Paint(sender As Object, e As PaintEventArgs) Handles BtnReject.Paint

        Dim g As Graphics = e.Graphics
        Dim callImage As New Bitmap(My.Resources.Phone2, 60, 60)

        BtnReject.DrawRoundRectControl(0, 0, Me.Width, Me.Height, 5)
        BtnReject.PaintGradient(e.Graphics, Color.Red, Color.DarkRed)
        g.DrawImage(callImage, 15, 0)

    End Sub

    Public Sub New(myPhoneStatus As SPhoneStatus)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        _frmCallPhoneStatus = myPhoneStatus
    End Sub
End Class
