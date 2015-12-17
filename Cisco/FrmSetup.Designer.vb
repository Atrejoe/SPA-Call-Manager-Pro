<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class FrmSetup
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
        Dim resources As System.ComponentModel.ComponentResourceManager = New System.ComponentModel.ComponentResourceManager(GetType(FrmSetup))
        Me.LbldebugAddress = New System.Windows.Forms.Label()
        Me.LblLocalIp = New System.Windows.Forms.Label()
        Me.Btnsave = New System.Windows.Forms.Button()
        Me.CmbLocalIP = New System.Windows.Forms.ComboBox()
        Me.TxtphoneIP = New System.Windows.Forms.TextBox()
        Me.LblPhoneIp = New System.Windows.Forms.Label()
        Me.btnImportCsv = New System.Windows.Forms.Button()
        Me.btnExportCsv = New System.Windows.Forms.Button()
        Me.OFDImport = New System.Windows.Forms.OpenFileDialog()
        Me.SFDExport = New System.Windows.Forms.SaveFileDialog()
        Me.lblCTIEnabled = New System.Windows.Forms.Label()
        Me.imgCTIEnabled = New System.Windows.Forms.PictureBox()
        Me.imgDebugFull = New System.Windows.Forms.PictureBox()
        Me.lblDebugLevelFull = New System.Windows.Forms.Label()
        Me.imgStationSet = New System.Windows.Forms.PictureBox()
        Me.lblStationNameSet = New System.Windows.Forms.Label()
        Me.imgLinksysKeySystemEnabled = New System.Windows.Forms.PictureBox()
        Me.lblLinksysKeySystemEnabled = New System.Windows.Forms.Label()
        Me.btnRecheck = New System.Windows.Forms.Button()
        Me.Button1 = New System.Windows.Forms.Button()
        Me.imgDebugServerSet = New System.Windows.Forms.PictureBox()
        Me.Label1 = New System.Windows.Forms.Label()
        Me.TxtSharedFolder = New System.Windows.Forms.TextBox()
        Me.btnsharedFolder = New System.Windows.Forms.Button()
        Me.SharedFBD = New System.Windows.Forms.FolderBrowserDialog()
        Me.txtpassword = New System.Windows.Forms.TextBox()
        Me.Label3 = New System.Windows.Forms.Label()
        CType(Me.imgCTIEnabled, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.imgDebugFull, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.imgStationSet, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.imgLinksysKeySystemEnabled, System.ComponentModel.ISupportInitialize).BeginInit()
        CType(Me.imgDebugServerSet, System.ComponentModel.ISupportInitialize).BeginInit()
        Me.SuspendLayout()
        '
        'LbldebugAddress
        '
        Me.LbldebugAddress.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.LbldebugAddress.AutoSize = True
        Me.LbldebugAddress.BackColor = System.Drawing.Color.Transparent
        Me.LbldebugAddress.ForeColor = System.Drawing.Color.White
        Me.LbldebugAddress.Location = New System.Drawing.Point(30, 275)
        Me.LbldebugAddress.Name = "LbldebugAddress"
        Me.LbldebugAddress.Size = New System.Drawing.Size(151, 13)
        Me.LbldebugAddress.TabIndex = 1
        Me.LbldebugAddress.Text = "Debug Server not set correctly"
        '
        'LblLocalIp
        '
        Me.LblLocalIp.AutoSize = True
        Me.LblLocalIp.BackColor = System.Drawing.Color.Transparent
        Me.LblLocalIp.ForeColor = System.Drawing.Color.White
        Me.LblLocalIp.Location = New System.Drawing.Point(7, 49)
        Me.LblLocalIp.Name = "LblLocalIp"
        Me.LblLocalIp.Size = New System.Drawing.Size(129, 13)
        Me.LblLocalIp.TabIndex = 2
        Me.LblLocalIp.Text = "PC IP Address to listen on"
        '
        'Btnsave
        '
        Me.Btnsave.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Btnsave.Location = New System.Drawing.Point(8, 403)
        Me.Btnsave.Name = "Btnsave"
        Me.Btnsave.Size = New System.Drawing.Size(265, 23)
        Me.Btnsave.TabIndex = 6
        Me.Btnsave.Text = "Save Settings"
        Me.Btnsave.UseVisualStyleBackColor = True
        '
        'CmbLocalIP
        '
        Me.CmbLocalIP.FormattingEnabled = True
        Me.CmbLocalIP.Location = New System.Drawing.Point(10, 64)
        Me.CmbLocalIP.Name = "CmbLocalIP"
        Me.CmbLocalIP.Size = New System.Drawing.Size(262, 21)
        Me.CmbLocalIP.TabIndex = 9
        '
        'TxtphoneIP
        '
        Me.TxtphoneIP.Location = New System.Drawing.Point(10, 23)
        Me.TxtphoneIP.Name = "TxtphoneIP"
        Me.TxtphoneIP.Size = New System.Drawing.Size(262, 20)
        Me.TxtphoneIP.TabIndex = 11
        '
        'LblPhoneIp
        '
        Me.LblPhoneIp.AutoSize = True
        Me.LblPhoneIp.BackColor = System.Drawing.Color.Transparent
        Me.LblPhoneIp.ForeColor = System.Drawing.Color.White
        Me.LblPhoneIp.Location = New System.Drawing.Point(7, 8)
        Me.LblPhoneIp.Name = "LblPhoneIp"
        Me.LblPhoneIp.Size = New System.Drawing.Size(39, 13)
        Me.LblPhoneIp.TabIndex = 10
        Me.LblPhoneIp.Text = "Label1"
        '
        'btnImportCsv
        '
        Me.btnImportCsv.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnImportCsv.Location = New System.Drawing.Point(9, 374)
        Me.btnImportCsv.Name = "btnImportCsv"
        Me.btnImportCsv.Size = New System.Drawing.Size(128, 23)
        Me.btnImportCsv.TabIndex = 12
        Me.btnImportCsv.Text = "Import CSV File"
        Me.btnImportCsv.UseVisualStyleBackColor = True
        '
        'btnExportCsv
        '
        Me.btnExportCsv.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnExportCsv.Location = New System.Drawing.Point(145, 374)
        Me.btnExportCsv.Name = "btnExportCsv"
        Me.btnExportCsv.Size = New System.Drawing.Size(128, 23)
        Me.btnExportCsv.TabIndex = 13
        Me.btnExportCsv.Text = "Export CSV File"
        Me.btnExportCsv.UseVisualStyleBackColor = True
        '
        'SFDExport
        '
        Me.SFDExport.DefaultExt = "*.csv"
        '
        'lblCTIEnabled
        '
        Me.lblCTIEnabled.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblCTIEnabled.AutoSize = True
        Me.lblCTIEnabled.BackColor = System.Drawing.Color.Transparent
        Me.lblCTIEnabled.ForeColor = System.Drawing.Color.White
        Me.lblCTIEnabled.Location = New System.Drawing.Point(30, 185)
        Me.lblCTIEnabled.Name = "lblCTIEnabled"
        Me.lblCTIEnabled.Size = New System.Drawing.Size(66, 13)
        Me.lblCTIEnabled.TabIndex = 14
        Me.lblCTIEnabled.Text = "CTI Enabled"
        '
        'imgCTIEnabled
        '
        Me.imgCTIEnabled.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.imgCTIEnabled.BackColor = System.Drawing.Color.Transparent
        Me.imgCTIEnabled.Image = Global.Pss.Cisco.My.Resources.Resources.Cross
        Me.imgCTIEnabled.Location = New System.Drawing.Point(8, 185)
        Me.imgCTIEnabled.Name = "imgCTIEnabled"
        Me.imgCTIEnabled.Size = New System.Drawing.Size(16, 16)
        Me.imgCTIEnabled.TabIndex = 15
        Me.imgCTIEnabled.TabStop = False
        '
        'imgDebugFull
        '
        Me.imgDebugFull.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.imgDebugFull.BackColor = System.Drawing.Color.Transparent
        Me.imgDebugFull.Image = Global.Pss.Cisco.My.Resources.Resources.Cross
        Me.imgDebugFull.Location = New System.Drawing.Point(8, 208)
        Me.imgDebugFull.Name = "imgDebugFull"
        Me.imgDebugFull.Size = New System.Drawing.Size(16, 16)
        Me.imgDebugFull.TabIndex = 17
        Me.imgDebugFull.TabStop = False
        '
        'lblDebugLevelFull
        '
        Me.lblDebugLevelFull.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblDebugLevelFull.AutoSize = True
        Me.lblDebugLevelFull.BackColor = System.Drawing.Color.Transparent
        Me.lblDebugLevelFull.ForeColor = System.Drawing.Color.White
        Me.lblDebugLevelFull.Location = New System.Drawing.Point(30, 208)
        Me.lblDebugLevelFull.Name = "lblDebugLevelFull"
        Me.lblDebugLevelFull.Size = New System.Drawing.Size(109, 13)
        Me.lblDebugLevelFull.TabIndex = 16
        Me.lblDebugLevelFull.Text = "Debug level set to full"
        '
        'imgStationSet
        '
        Me.imgStationSet.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.imgStationSet.BackColor = System.Drawing.Color.Transparent
        Me.imgStationSet.Image = Global.Pss.Cisco.My.Resources.Resources.Cross
        Me.imgStationSet.Location = New System.Drawing.Point(8, 231)
        Me.imgStationSet.Name = "imgStationSet"
        Me.imgStationSet.Size = New System.Drawing.Size(16, 16)
        Me.imgStationSet.TabIndex = 19
        Me.imgStationSet.TabStop = False
        '
        'lblStationNameSet
        '
        Me.lblStationNameSet.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblStationNameSet.AutoSize = True
        Me.lblStationNameSet.BackColor = System.Drawing.Color.Transparent
        Me.lblStationNameSet.ForeColor = System.Drawing.Color.White
        Me.lblStationNameSet.Location = New System.Drawing.Point(30, 231)
        Me.lblStationNameSet.Name = "lblStationNameSet"
        Me.lblStationNameSet.Size = New System.Drawing.Size(110, 13)
        Me.lblStationNameSet.TabIndex = 18
        Me.lblStationNameSet.Text = "Station Name Not Set"
        '
        'imgLinksysKeySystemEnabled
        '
        Me.imgLinksysKeySystemEnabled.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.imgLinksysKeySystemEnabled.BackColor = System.Drawing.Color.Transparent
        Me.imgLinksysKeySystemEnabled.Image = Global.Pss.Cisco.My.Resources.Resources.Cross
        Me.imgLinksysKeySystemEnabled.Location = New System.Drawing.Point(8, 253)
        Me.imgLinksysKeySystemEnabled.Name = "imgLinksysKeySystemEnabled"
        Me.imgLinksysKeySystemEnabled.Size = New System.Drawing.Size(16, 16)
        Me.imgLinksysKeySystemEnabled.TabIndex = 21
        Me.imgLinksysKeySystemEnabled.TabStop = False
        '
        'lblLinksysKeySystemEnabled
        '
        Me.lblLinksysKeySystemEnabled.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.lblLinksysKeySystemEnabled.AutoSize = True
        Me.lblLinksysKeySystemEnabled.BackColor = System.Drawing.Color.Transparent
        Me.lblLinksysKeySystemEnabled.ForeColor = System.Drawing.Color.White
        Me.lblLinksysKeySystemEnabled.Location = New System.Drawing.Point(30, 253)
        Me.lblLinksysKeySystemEnabled.Name = "lblLinksysKeySystemEnabled"
        Me.lblLinksysKeySystemEnabled.Size = New System.Drawing.Size(142, 13)
        Me.lblLinksysKeySystemEnabled.TabIndex = 20
        Me.lblLinksysKeySystemEnabled.Text = "Linksys Key System Enabled"
        '
        'btnRecheck
        '
        Me.btnRecheck.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.btnRecheck.Location = New System.Drawing.Point(10, 317)
        Me.btnRecheck.Name = "btnRecheck"
        Me.btnRecheck.Size = New System.Drawing.Size(263, 23)
        Me.btnRecheck.TabIndex = 22
        Me.btnRecheck.Text = "Recheck Configuration"
        Me.btnRecheck.UseVisualStyleBackColor = True
        '
        'Button1
        '
        Me.Button1.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.Button1.Location = New System.Drawing.Point(10, 345)
        Me.Button1.Name = "Button1"
        Me.Button1.Size = New System.Drawing.Size(263, 23)
        Me.Button1.TabIndex = 23
        Me.Button1.Text = "License Maintenance"
        Me.Button1.UseVisualStyleBackColor = True
        '
        'imgDebugServerSet
        '
        Me.imgDebugServerSet.Anchor = CType(((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Left) _
            Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
        Me.imgDebugServerSet.BackColor = System.Drawing.Color.Transparent
        Me.imgDebugServerSet.Image = Global.Pss.Cisco.My.Resources.Resources.Cross
        Me.imgDebugServerSet.Location = New System.Drawing.Point(8, 275)
        Me.imgDebugServerSet.Name = "imgDebugServerSet"
        Me.imgDebugServerSet.Size = New System.Drawing.Size(16, 16)
        Me.imgDebugServerSet.TabIndex = 24
        Me.imgDebugServerSet.TabStop = False
        '
        'Label1
        '
        Me.Label1.AutoSize = True
        Me.Label1.BackColor = System.Drawing.Color.Transparent
        Me.Label1.ForeColor = System.Drawing.Color.White
        Me.Label1.Location = New System.Drawing.Point(7, 91)
        Me.Label1.Name = "Label1"
        Me.Label1.Size = New System.Drawing.Size(118, 13)
        Me.Label1.TabIndex = 25
        Me.Label1.Text = "Shared Directory Folder"
        '
        'TxtSharedFolder
        '
        Me.TxtSharedFolder.Location = New System.Drawing.Point(9, 109)
        Me.TxtSharedFolder.Name = "TxtSharedFolder"
        Me.TxtSharedFolder.Size = New System.Drawing.Size(231, 20)
        Me.TxtSharedFolder.TabIndex = 26
        '
        'btnsharedFolder
        '
        Me.btnsharedFolder.Location = New System.Drawing.Point(243, 107)
        Me.btnsharedFolder.Name = "btnsharedFolder"
        Me.btnsharedFolder.Size = New System.Drawing.Size(28, 23)
        Me.btnsharedFolder.TabIndex = 27
        Me.btnsharedFolder.Text = "..."
        Me.btnsharedFolder.UseVisualStyleBackColor = True
        '
        'txtpassword
        '
        Me.txtpassword.Location = New System.Drawing.Point(10, 152)
        Me.txtpassword.Name = "txtpassword"
        Me.txtpassword.PasswordChar = Global.Microsoft.VisualBasic.ChrW(8226)
        Me.txtpassword.Size = New System.Drawing.Size(261, 20)
        Me.txtpassword.TabIndex = 31
        '
        'Label3
        '
        Me.Label3.AutoSize = True
        Me.Label3.BackColor = System.Drawing.Color.Transparent
        Me.Label3.ForeColor = System.Drawing.Color.White
        Me.Label3.Location = New System.Drawing.Point(7, 138)
        Me.Label3.Name = "Label3"
        Me.Label3.Size = New System.Drawing.Size(85, 13)
        Me.Label3.TabIndex = 30
        Me.Label3.Text = "Admin Password"
        '
        'FrmSetup
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.BackColor = System.Drawing.SystemColors.ActiveCaptionText
        Me.ClientSize = New System.Drawing.Size(284, 438)
        Me.Controls.Add(Me.txtpassword)
        Me.Controls.Add(Me.Label3)
        Me.Controls.Add(Me.btnsharedFolder)
        Me.Controls.Add(Me.TxtSharedFolder)
        Me.Controls.Add(Me.Label1)
        Me.Controls.Add(Me.imgDebugServerSet)
        Me.Controls.Add(Me.Button1)
        Me.Controls.Add(Me.btnRecheck)
        Me.Controls.Add(Me.imgLinksysKeySystemEnabled)
        Me.Controls.Add(Me.lblLinksysKeySystemEnabled)
        Me.Controls.Add(Me.imgStationSet)
        Me.Controls.Add(Me.lblStationNameSet)
        Me.Controls.Add(Me.imgDebugFull)
        Me.Controls.Add(Me.lblDebugLevelFull)
        Me.Controls.Add(Me.imgCTIEnabled)
        Me.Controls.Add(Me.lblCTIEnabled)
        Me.Controls.Add(Me.btnExportCsv)
        Me.Controls.Add(Me.btnImportCsv)
        Me.Controls.Add(Me.TxtphoneIP)
        Me.Controls.Add(Me.LblPhoneIp)
        Me.Controls.Add(Me.CmbLocalIP)
        Me.Controls.Add(Me.Btnsave)
        Me.Controls.Add(Me.LblLocalIp)
        Me.Controls.Add(Me.LbldebugAddress)
        Me.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog
        Me.Icon = CType(resources.GetObject("$this.Icon"), System.Drawing.Icon)
        Me.MaximizeBox = False
        Me.MinimizeBox = False
        Me.Name = "FrmSetup"
        Me.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen
        Me.Text = "SPA Call Manager Pro Setup"
        CType(Me.imgCTIEnabled, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.imgDebugFull, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.imgStationSet, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.imgLinksysKeySystemEnabled, System.ComponentModel.ISupportInitialize).EndInit()
        CType(Me.imgDebugServerSet, System.ComponentModel.ISupportInitialize).EndInit()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Friend WithEvents LbldebugAddress As System.Windows.Forms.Label
    Friend WithEvents LblLocalIp As System.Windows.Forms.Label
    Friend WithEvents Btnsave As System.Windows.Forms.Button
    Friend WithEvents CmbLocalIP As System.Windows.Forms.ComboBox
    Friend WithEvents TxtphoneIP As System.Windows.Forms.TextBox
    Friend WithEvents LblPhoneIp As System.Windows.Forms.Label
    Friend WithEvents btnImportCsv As System.Windows.Forms.Button
    Friend WithEvents btnExportCsv As System.Windows.Forms.Button
    Friend WithEvents OFDImport As System.Windows.Forms.OpenFileDialog
    Friend WithEvents SFDExport As System.Windows.Forms.SaveFileDialog
    Friend WithEvents lblCTIEnabled As System.Windows.Forms.Label
    Friend WithEvents imgCTIEnabled As System.Windows.Forms.PictureBox
    Friend WithEvents imgDebugFull As System.Windows.Forms.PictureBox
    Friend WithEvents lblDebugLevelFull As System.Windows.Forms.Label
    Friend WithEvents imgStationSet As System.Windows.Forms.PictureBox
    Friend WithEvents lblStationNameSet As System.Windows.Forms.Label
    Friend WithEvents imgLinksysKeySystemEnabled As System.Windows.Forms.PictureBox
    Friend WithEvents lblLinksysKeySystemEnabled As System.Windows.Forms.Label
    Friend WithEvents btnRecheck As System.Windows.Forms.Button
    Friend WithEvents Button1 As System.Windows.Forms.Button
    Friend WithEvents imgDebugServerSet As System.Windows.Forms.PictureBox
    Friend WithEvents Label1 As System.Windows.Forms.Label
    Friend WithEvents TxtSharedFolder As System.Windows.Forms.TextBox
    Friend WithEvents btnsharedFolder As System.Windows.Forms.Button
    Friend WithEvents SharedFBD As System.Windows.Forms.FolderBrowserDialog
    Friend WithEvents txtpassword As System.Windows.Forms.TextBox
    Friend WithEvents Label3 As System.Windows.Forms.Label
End Class
