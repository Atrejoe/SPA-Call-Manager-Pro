Imports System.Drawing.Drawing2D

Public Class FrmCall

    Dim CallBackColor As Color = Color.LightSlateGray
    Dim CallBorderColor As Color = Color.Black
    Dim FrmCallPhoneStatus As SPhoneStatus

    Private Sub FrmCall_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        Dim g As Graphics = Me.CreateGraphics
        g.SmoothingMode = SmoothingMode.AntiAlias
        DrawRoundRectForm(0, 0, Me.Width, Me.Height, 15)
        Me.BackColor = CallBorderColor

    End Sub

    Private Sub FrmCall_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint

        Dim g As Graphics = e.Graphics
        
        Dim textFont As New Font("trebuchet MS", 12, FontStyle.Bold)
       
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias

        Dim fillBrush As New LinearGradientBrush(New RectangleF(0, 0, me.Width, me.Height), Color.White, Color.Gray, LinearGradientMode.Vertical)
        drawRoundRectFill(g, fillBrush, 2, 2, me.Width - 4, me.Height - 4, 15)

        g.DrawString("Incoming Call Line: " & FrmCallPhoneStatus.Id, textFont, Brushes.Black, 10, 10)

        If FrmCallPhoneStatus.CallerName = "" Then
            g.DrawString(FrmCallPhoneStatus.CallerNumber, textFont, Brushes.Black, 10, 30)
        Else
            g.DrawString(FrmCallPhoneStatus.CallerName, textFont, Brushes.Black, 10, 30)
        End If

    End Sub

    Private Sub BtnAnswer_Click(sender As Object, e As EventArgs) Handles BtnAnswer.Click
        
        Dim CallString As String = PhoneAction(eAction.Answer, FrmCallPhoneStatus, MyPhoneSettings)

        SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
    End Sub

    Private Sub BtnAnswer_MouseEnter(sender As Object, e As EventArgs) Handles BtnAnswer.MouseEnter

        Me.Cursor = Cursors.Hand

    End Sub

    Private Sub BtnAnswer_MouseLeave(sender As Object, e As EventArgs) Handles BtnAnswer.MouseLeave

        Me.Cursor = Cursors.Default

    End Sub

    Private Sub BtnAnswer_Paint(sender As Object, e As PaintEventArgs) Handles BtnAnswer.Paint

        Dim g As Graphics = e.Graphics
        Dim CallImage As New Bitmap(My.Resources.Phone1, 60, 60)
        
        BtnAnswer.DrawRoundRectControl(0, 0, me.width, me.height, 5)
        BtnAnswer.PaintGradient(e.Graphics, Color.Green, Color.DarkGreen)
        g.DrawImage(CallImage, 15, 0)


    End Sub

    Private Sub BtnReject_Click(sender As Object, e As EventArgs) Handles BtnReject.Click

        Dim callString As String = PhoneAction(eAction.Reject, FrmCallPhoneStatus, MyPhoneSettings)

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
        Dim CallImage As New Bitmap(My.Resources.Phone2, 60, 60)
        
        BtnReject.DrawRoundRectControl(0, 0, me.width, me.height, 5)
        BtnReject.PaintGradient(e.Graphics, Color.Red, Color.DarkRed)
        g.DrawImage(CallImage, 15, 0)

    End Sub

    Public Sub New(MyPhoneStatus As SPhoneStatus)

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        FrmCallPhoneStatus = MyPhoneStatus     
    End Sub
End Class
