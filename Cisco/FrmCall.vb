Imports System.Drawing.Drawing2D

Public Class FrmCall

    Dim CallBackColor As Color = Color.LightSlateGray
    Dim CallBorderColor As Color = Color.Black
    Dim FrmCallPhoneStatus As ClsPhone.sPhoneStatus

    Private Sub FrmCall_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Dim g As Graphics = Me.CreateGraphics
        g.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias
        DrawRoundRectForm(Me, 0, 0, Me.Width, Me.Height, 15)
        Me.BackColor = CallBorderColor

    End Sub

    Private Sub FrmCall_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint

        Dim g As Graphics = e.Graphics
        Dim stringSize As New SizeF()
        Dim textFont As New Font("trebuchet MS", 12, FontStyle.Bold)
       
        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        Dim fillBrush As New System.Drawing.Drawing2D.LinearGradientBrush(New RectangleF(0, 0, me.Width, me.Height), Color.White, Color.Gray, LinearGradientMode.Vertical)
        drawRoundRectFill(g, fillBrush, 2, 2, me.Width - 4, me.Height - 4, 15)

        g.DrawString("Incoming Call Line: " & FrmCallPhoneStatus.Id, textFont, Brushes.Black, 10, 10)

        If FrmCallPhoneStatus.CallerName = "" Then
            g.DrawString(FrmCallPhoneStatus.CallerNumber, textFont, Brushes.Black, 10, 30)
        Else
            g.DrawString(FrmCallPhoneStatus.CallerName, textFont, Brushes.Black, 10, 30)
        End If

    End Sub

    Private Sub BtnAnswer_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnAnswer.Click

        Dim MycallControl As New CallControl
        Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Answer, FrmCallPhoneStatus, MyPhoneSettings)

        FrmMain.MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
        MycallControl = Nothing


    End Sub

    Private Sub BtnAnswer_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnAnswer.MouseEnter

        Me.Cursor = Cursors.Hand

    End Sub

    Private Sub BtnAnswer_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnAnswer.MouseLeave

        Me.Cursor = Cursors.Default

    End Sub

    Private Sub BtnAnswer_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles BtnAnswer.Paint

        Dim g As Graphics = e.Graphics
        Dim CallImage As New Bitmap(My.Resources.Phone1, 60, 60)
        Dim CallImageNew As Graphics = Graphics.FromImage(CallImage)

        DrawRoundRectControl(BtnAnswer, 0, 0, me.width, me.height, 5)
        BtnAnswer.PaintGradient(e.Graphics, Color.Green, Color.DarkGreen)
        g.DrawImage(CallImage, 15, 0)


    End Sub

    Private Sub BtnReject_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnReject.Click

        Dim MycallControl As New CallControl
        Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Reject, FrmCallPhoneStatus, MyPhoneSettings)

        FrmMain.MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
        MycallControl = Nothing

    End Sub

    Private Sub BtnReject_MouseEnter(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnReject.MouseEnter

        Me.Cursor = Cursors.Hand

    End Sub

    Private Sub BtnReject_MouseLeave(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnReject.MouseLeave

        Me.Cursor = Cursors.Default

    End Sub

    Private Sub BtnReject_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles BtnReject.Paint

        Dim g As Graphics = e.Graphics
        Dim CallImage As New Bitmap(My.Resources.Phone2, 60, 60)
        Dim CallImageNew As Graphics = Graphics.FromImage(CallImage)

        DrawRoundRectControl(BtnReject, 0, 0, me.width, me.height, 5)
        BtnReject.PaintGradient(e.Graphics, Color.Red, Color.DarkRed)
        g.DrawImage(CallImage, 15, 0)

    End Sub

    Public Sub New(ByVal MyPhoneStatus As ClsPhone.sPhoneStatus)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        FrmCallPhoneStatus = MyPhoneStatus

       

    End Sub
End Class
