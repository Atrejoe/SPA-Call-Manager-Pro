<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmCall
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
        Me.BtnAnswer = New System.Windows.Forms.Button()
        Me.BtnReject = New System.Windows.Forms.Button()
        Me.SuspendLayout()
        '
        'BtnAnswer
        '
        Me.BtnAnswer.BackColor = System.Drawing.Color.LimeGreen
        Me.BtnAnswer.Location = New System.Drawing.Point(10, 54)
        Me.BtnAnswer.Name = "BtnAnswer"
        Me.BtnAnswer.Size = New System.Drawing.Size(90, 60)
        Me.BtnAnswer.TabIndex = 0
        Me.BtnAnswer.UseVisualStyleBackColor = False
        '
        'BtnReject
        '
        Me.BtnReject.BackColor = System.Drawing.Color.LimeGreen
        Me.BtnReject.Location = New System.Drawing.Point(124, 54)
        Me.BtnReject.Name = "BtnReject"
        Me.BtnReject.Size = New System.Drawing.Size(90, 60)
        Me.BtnReject.TabIndex = 1
        Me.BtnReject.UseVisualStyleBackColor = False
        '
        'FrmCall
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.Color.DimGray
        Me.ClientSize = New System.Drawing.Size(223, 123)
        Me.Controls.Add(Me.BtnReject)
        Me.Controls.Add(Me.BtnAnswer)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "FrmCall"
        Me.Text = "FrmCall"
        Me.ResumeLayout(False)

    End Sub
    Friend WithEvents BtnAnswer As System.Windows.Forms.Button
    Friend WithEvents BtnReject As System.Windows.Forms.Button
End Class
