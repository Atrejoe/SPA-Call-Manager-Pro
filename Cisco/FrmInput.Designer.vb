﻿<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmInput
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
        Me.TxtphoneIP = New System.Windows.Forms.TextBox()
        Me.LblPhoneIp = New System.Windows.Forms.Label()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'TxtphoneIP
        '
        Me.TxtphoneIP.Location = New System.Drawing.Point(12, 25)
        Me.TxtphoneIP.Name = "TxtphoneIP"
        Me.TxtphoneIP.Size = New System.Drawing.Size(262, 20)
        Me.TxtphoneIP.TabIndex = 13
        '
        'LblPhoneIp
        '
        Me.LblPhoneIp.AutoSize = True
        Me.LblPhoneIp.BackColor = System.Drawing.Color.Transparent
        Me.LblPhoneIp.ForeColor = System.Drawing.Color.White
        Me.LblPhoneIp.Location = New System.Drawing.Point(9, 10)
        Me.LblPhoneIp.Name = "LblPhoneIp"
        Me.LblPhoneIp.Size = New System.Drawing.Size(85, 13)
        Me.LblPhoneIp.TabIndex = 12
        Me.LblPhoneIp.Text = "Admin Password"
        '
        'Button1
        '
        Me.Button1.Location = New System.Drawing.Point(219, 51)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(55, 28)
        Me.Button1.TabIndex = 14
        Me.Button1.Text = "OK"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'FrmInput
        '
        Me.AccessibleRole = System.Windows.Forms.AccessibleRole.TitleBar
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(284, 85)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.TxtphoneIP)
        Me.Controls.Add(Me.LblPhoneIp)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow
        Me.Name = "FrmInput"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent
        Me.Text = "Input Admin Password"
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents TxtphoneIP As System.Windows.Forms.TextBox
    Friend WithEvents LblPhoneIp As System.Windows.Forms.Label
    Friend WithEvents Button1 As System.Windows.Forms.Button
End Class