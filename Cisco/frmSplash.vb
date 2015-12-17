Imports System.Drawing
Imports System.Drawing.Drawing2D

Public Class frmSplash

    Private Declare Function SetWindowRgn Lib "user32" (ByVal hWnd As Integer, ByVal hRgn As Integer, ByVal bRedraw As Integer) As Integer
    Private Declare Function CreateRoundRectRgn Lib "gdi32" Alias "CreateRoundRectRgn" (ByVal X1 As Integer, ByVal Y1 As Integer, ByVal X2 As Integer, ByVal Y2 As Integer, ByVal X3 As Integer, ByVal Y3 As Integer) As Integer
    Dim splashBackColor As Color = Color.White
    Dim splashBorderColor As Color = Color.FromArgb(102, 152, 1)

    Private Sub drawRoundRectFill(ByVal g As Graphics, ByVal b As Brush, ByVal x As Integer, ByVal y As Integer, ByVal width As Integer, ByVal height As Integer, ByVal radius As Integer)

        Dim gp As GraphicsPath = New GraphicsPath

        gp.AddLine(x + radius, y, x + width - radius, y)
        gp.AddArc(x + width - radius, y, radius, radius, 270, 90)
        gp.AddLine(x + width, y + radius, x + width, y + height - radius)
        gp.AddArc(x + width - radius, y + height - radius, radius, radius, 0, 90)
        gp.AddLine(x + width - radius, y + height, x + radius, y + height)
        gp.AddArc(x, y + height - radius, radius, radius, 90, 90)
        gp.AddLine(x, y + height - radius, x, y + radius)
        gp.AddArc(x, y, radius, radius, 180, 90)

        gp.CloseFigure()
        g.FillPath(b, gp)
        gp.Dispose()

    End Sub

    Private Sub frmSplash_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Me.BackColor = splashBorderColor
        SetWindowRgn(Me.Handle, CreateRoundRectRgn(0, 0, Me.Width, Me.Height, 25, 25), True)

        'Application title
        If My.Application.Info.Title <> "" Then
            lblAppName.Text = My.Application.Info.Title
        Else
            'If the application title is missing, use the application name, without the extension
            lblAppName.Text = System.IO.Path.GetFileNameWithoutExtension(My.Application.Info.AssemblyName)
        End If

        'The following version integers are stored in the Assembly Version area of the Assembly Information window.
        'As far as I can tell, the File Version area doesn't have a directly accessible set of variables
        lblVersion.Text = "Version: " & My.Application.Info.Version.Major & "." & My.Application.Info.Version.Minor & "." & My.Application.Info.Version.Build & "." & My.Application.Info.Version.Revision

        Dim info As String = ""
        info = "© Phoenix Software Solutions Ltd., 2011"
        info = info & vbCrLf & vbCrLf
        info = info & "Software written by Graham Caplin" & vbCrLf & "and Richard Bettison"
        info = info & vbCrLf & vbCrLf
        info = info & "For more information email: support@phoenixsoftwaresolutions.co.uk"
        info = info & vbCrLf & vbCrLf
        info = info & "No part of this software may be copied or used without express written consent from Phoenix Software Solutions Ltd."

        lblInformation.Text = info

    End Sub

    Private Sub frmSplash_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint

        Dim g As Graphics = e.Graphics
        Dim stringSize As New SizeF()

        e.Graphics.SmoothingMode = Drawing2D.SmoothingMode.AntiAlias

        Dim fillBrush As New SolidBrush(splashBackColor)
        drawRoundRectFill(g, fillBrush, 5, 5, Me.Width - 12, Me.Height - 12, 19)

        lblAppName.BackColor = splashBackColor
        lblVersion.BackColor = splashBackColor
        lblInformation.BackColor = splashBackColor
        lblCompany1.BackColor = splashBackColor
        lblCompany2.BackColor = splashBackColor
        lblCompany3.BackColor = splashBackColor
        pbSoftwareLogo.BackColor = splashBackColor
        pbCompanyLogo.BackColor = splashBackColor

    End Sub

End Class