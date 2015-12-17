<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class frmSplash
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
        Me.lblAppName = New System.Windows.Forms.Label()
        Me.pbSoftwareLogo = New System.Windows.Forms.PictureBox()
        Me.pbCompanyLogo = New System.Windows.Forms.PictureBox()
        Me.lblVersion = New System.Windows.Forms.Label()
        Me.lblInformation = New System.Windows.Forms.Label()
        Me.lblCompany2 = New System.Windows.Forms.Label()
        Me.lblCompany3 = New System.Windows.Forms.Label()
        Me.lblCompany1 = New System.Windows.Forms.Label()
        CType(Me.pbSoftwareLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.pbCompanyLogo, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'lblAppName
        '
        Me.lblAppName.AutoSize = True
        Me.lblAppName.BackColor = System.Drawing.Color.White
        Me.lblAppName.Font = New System.Drawing.Font("Trebuchet MS", 18.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblAppName.ForeColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(1, Byte), Integer))
        Me.lblAppName.Location = New System.Drawing.Point(33, 230)
        Me.lblAppName.Name = "lblAppName"
        Me.lblAppName.Size = New System.Drawing.Size(161, 29)
        Me.lblAppName.TabIndex = 1
        Me.lblAppName.Text = "Hire Manager"
        Me.lblAppName.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'pbSoftwareLogo
        '
        Me.pbSoftwareLogo.BackColor = System.Drawing.Color.White
        Me.pbSoftwareLogo.Image = Global.WindowsApplication1.My.Resources.Resources.phone
        Me.pbSoftwareLogo.Location = New System.Drawing.Point(21, 9)
        Me.pbSoftwareLogo.Name = "pbSoftwareLogo"
        Me.pbSoftwareLogo.Size = New System.Drawing.Size(184, 218)
        Me.pbSoftwareLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.pbSoftwareLogo.TabIndex = 2
        Me.pbSoftwareLogo.TabStop = False
        '
        'pbCompanyLogo
        '
        Me.pbCompanyLogo.Image = Global.WindowsApplication1.My.Resources.Resources.phone
        Me.pbCompanyLogo.Location = New System.Drawing.Point(417, 225)
        Me.pbCompanyLogo.Name = "pbCompanyLogo"
        Me.pbCompanyLogo.Size = New System.Drawing.Size(67, 58)
        Me.pbCompanyLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom
        Me.pbCompanyLogo.TabIndex = 0
        Me.pbCompanyLogo.TabStop = False
        '
        'lblVersion
        '
        Me.lblVersion.Font = New System.Drawing.Font("Trebuchet MS", 9.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblVersion.ForeColor = System.Drawing.Color.FromArgb(CType(CType(102, Byte), Integer), CType(CType(152, Byte), Integer), CType(CType(1, Byte), Integer))
        Me.lblVersion.Location = New System.Drawing.Point(30, 266)
        Me.lblVersion.Name = "lblVersion"
        Me.lblVersion.Size = New System.Drawing.Size(166, 17)
        Me.lblVersion.TabIndex = 4
        Me.lblVersion.Text = "Version:"
        Me.lblVersion.TextAlign = System.Drawing.ContentAlignment.TopCenter
        '
        'lblInformation
        '
        Me.lblInformation.Font = New System.Drawing.Font("Microsoft Sans Serif", 9.5!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblInformation.Location = New System.Drawing.Point(211, 25)
        Me.lblInformation.Name = "lblInformation"
        Me.lblInformation.Size = New System.Drawing.Size(262, 185)
        Me.lblInformation.TabIndex = 5
        Me.lblInformation.Text = "lblInformation"
        Me.lblInformation.TextAlign = System.Drawing.ContentAlignment.MiddleCenter
        '
        'lblCompany2
        '
        Me.lblCompany2.AutoSize = True
        Me.lblCompany2.Font = New System.Drawing.Font("Verdana", 18.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCompany2.ForeColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(161, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.lblCompany2.Location = New System.Drawing.Point(298, 237)
        Me.lblCompany2.Name = "lblCompany2"
        Me.lblCompany2.Size = New System.Drawing.Size(121, 29)
        Me.lblCompany2.TabIndex = 6
        Me.lblCompany2.Text = "Phoenix"
        Me.lblCompany2.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblCompany3
        '
        Me.lblCompany3.AutoSize = True
        Me.lblCompany3.Font = New System.Drawing.Font("Verdana", 10.0!, CType((System.Drawing.FontStyle.Bold Or System.Drawing.FontStyle.Italic), System.Drawing.FontStyle), System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCompany3.ForeColor = System.Drawing.Color.FromArgb(CType(CType(224, Byte), Integer), CType(CType(161, Byte), Integer), CType(CType(55, Byte), Integer))
        Me.lblCompany3.Location = New System.Drawing.Point(261, 266)
        Me.lblCompany3.Name = "lblCompany3"
        Me.lblCompany3.Size = New System.Drawing.Size(156, 17)
        Me.lblCompany3.TabIndex = 7
        Me.lblCompany3.Text = "Software Solutions"
        Me.lblCompany3.TextAlign = System.Drawing.ContentAlignment.MiddleRight
        '
        'lblCompany1
        '
        Me.lblCompany1.AutoSize = True
        Me.lblCompany1.Font = New System.Drawing.Font("Trebuchet MS", 8.5!, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.lblCompany1.ForeColor = System.Drawing.Color.DimGray
        Me.lblCompany1.Location = New System.Drawing.Point(343, 225)
        Me.lblCompany1.Name = "lblCompany1"
        Me.lblCompany1.Size = New System.Drawing.Size(73, 18)
        Me.lblCompany1.TabIndex = 8
        Me.lblCompany1.Text = "designed by"
        '
        'frmSplash
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.Control
        Me.ClientSize = New System.Drawing.Size(505, 300)
        Me.Controls.Add(Me.pbCompanyLogo)
        Me.Controls.Add(Me.lblCompany1)
        Me.Controls.Add(Me.lblCompany3)
        Me.Controls.Add(Me.lblCompany2)
        Me.Controls.Add(Me.lblInformation)
        Me.Controls.Add(Me.lblVersion)
        Me.Controls.Add(Me.pbSoftwareLogo)
        Me.Controls.Add(Me.lblAppName)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None
        Me.Name = "frmSplash"
        Me.ShowInTaskbar = False
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "Form2"
        CType(Me.pbSoftwareLogo, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.pbCompanyLogo, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents pbCompanyLogo As System.Windows.Forms.PictureBox
    Friend WithEvents lblAppName As System.Windows.Forms.Label
    Friend WithEvents pbSoftwareLogo As System.Windows.Forms.PictureBox
    Friend WithEvents lblVersion As System.Windows.Forms.Label
    Friend WithEvents lblInformation As System.Windows.Forms.Label
    Friend WithEvents lblCompany2 As System.Windows.Forms.Label
    Friend WithEvents lblCompany3 As System.Windows.Forms.Label
    Friend WithEvents lblCompany1 As System.Windows.Forms.Label
End Class
