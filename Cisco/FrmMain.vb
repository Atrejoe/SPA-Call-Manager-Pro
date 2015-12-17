Imports System.Drawing.Drawing2D
Imports System.IO
Imports System.Net
Imports Microsoft.VisualBasic

Public Class FrmMain

    Dim newFrmCallLine1 As FrmCall 'popup form for line1
    Dim newFrmCallLine2 As FrmCall 'popup form for line2
    Dim newFrmCallLine3 As FrmCall 'popup form for line3
    Dim newFrmCallLine4 As FrmCall 'popup form for line4
    Dim FrmFade(4) As Boolean ''Sets if fade is enabled when form closes
    Dim HoldFlash(4) As Boolean ''Sets if fade is enabled when form closes
    Dim LinePhoneStatus(4) As ClsPhone.sPhoneStatus ' status of each line object
    Public WithEvents clpbrd As New ClipBoardMonitor ' monitors the clipboard for telephone numbers
    Public WithEvents MyPhone As New ClsPhone 'Phone class that handles communication with the phone
    Dim index As Integer = 0

    Private Sub FrmMain_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        CheckForExistingInstance()

        ' sets the app data folder and creates one if there isnt one already
        dataDir = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        If Directory.Exists(dataDir & "\CiscoPhone") = False Then
            Directory.CreateDirectory(dataDir & "\CiscoPhone")
        End If

        'Get stored settings for the phone and local Ip address of the PC
        MyStoredPhoneSettings = GetStoredSettings()

        'Me.Text = "SPA Call Manager Pro - " & MyStoredPhoneSettings.StationName & " - " & MyStoredPhoneSettings.PhoneModel & "-" & MyStoredPhoneSettings.PhoneSoftwareVersion

        If MyStoredPhoneSettings.PhoneIP = "" Then
            'If the Phone IP Address has not yet been populated, first show the setup dialog with some default values and then retreive the values for use afterwards.

            FrmSetup.LblLocalIp.Text = "Local IP Address"
            FrmSetup.CmbLocalIP.Items.AddRange(MyPhone.GetLocalIp)
            FrmSetup.CmbLocalIP.SelectedIndex = 0
            FrmSetup.LblPhoneIp.Text = "Phone IP Address"
            If FrmSetup.ShowDialog() = Windows.Forms.DialogResult.Cancel Then
                Me.Close()
            End If

            'Now that we have shown the form dialog, retreive the settings for use by the program
            MyStoredPhoneSettings = GetStoredSettings()
        End If

        'Try and ping the IP address of the phone before trying to communicate with it as this just casues a long delay waiting for things to timeout.
        If My.Computer.Network.Ping(MyStoredPhoneSettings.PhoneIP) Then
            'download phone settings
            MyPhoneSettings.password = MyStoredPhoneSettings.password
            MyPhone.password = MyPhoneSettings.password
            LoginPassword = MyPhoneSettings.password
            MyPhoneSettings = MyPhone.DownloadPhoneSettings(MyStoredPhoneSettings.PhoneIP)

            'check for incorrect data
            If MyStoredPhoneSettings.LocalIP <> MyPhoneSettings.Debug_Server_Address Then MsgBox("The ""Debug Server Address"" specified in the phone setup isn't the same as this PC IP address.  The phone will be unable to send status updates to the PC until this is corrected on handset preparation.", MsgBoxStyle.Exclamation, "SPA Call Manager Pro")
            If MyPhoneSettings.CTI_Enable = "No" Then MsgBox("CTI is not enabled on this handset.  SPA Call Manager Pro will be unable to initiate any calls for you until this setting is enabled.  Please see the support pages on www.spacallmanager.com for guidance on handset preparation.", MsgBoxStyle.Exclamation, "SPA Call Manager Pro")
            If MyPhoneSettings.DebugLevel <> "full" Then MsgBox("The Debug Level on the phone is not set to ""Full"".  SPA Call Manager Pro will not receive detailed status updates from the handset.  Please see the support pages on www.spacallmanager.com for guidance on handset preparation.", MsgBoxStyle.Exclamation, "SPA Call Manager Pro")
            If MyPhoneSettings.StationName = vbLf Then MsgBox("The station name has not been set on the phone, if this setting is not populated SPA Call Manager Pro will be unable to send commands to the handset.  Please see the support pages on www.spacallmanager.com for guidance on handset preparation.", MsgBoxStyle.Exclamation, "SPA Call Manager Pro")
            If MyPhoneSettings.LinksysKeySystem <> "Yes" Then MsgBox("Linksys Key System is not enabled on this handset.  SPA Call Manager Pro will be unable to initiate any calls for you until this setting is enabled.  Please see the support pages on www.spacallmanager.com for guidance on handset preparation.", MsgBoxStyle.Exclamation, "SPA Call Manager Pro")
            'save settings to registery
            MyPhoneSettings.PhoneIP = MyStoredPhoneSettings.PhoneIP
            MyPhoneSettings.PhonePort = MyStoredPhoneSettings.PhonePort
            MyPhoneSettings.LocalPort = MyStoredPhoneSettings.LocalPort
          
            MyPhone.IpPort = MyStoredPhoneSettings.LocalPort
            MyPhone.Startlistening()


            TmrFadeNotification.Enabled = True
            For x As Integer = 1 To 4
                FrmFade(x) = False
            Next

            'retrieve call data from phone
            GetPhoneDir("http://" & MyStoredPhoneSettings.PhoneIP & "/pdir.htm")
            GetPhoneCalled("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")
            GetPhoneAnswered("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")
            GetPhoneMissed("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")
            LoadPhoneBook(dataDir & "\CiscoPhone\Phonebook.csv")
            If SharedDataDir <> "" Then
                FSW.Path = SharedDataDir
                FrmSetup.TxtSharedFolder.Text = SharedDataDir
                LoadSharedPhoneBook(SharedDataDir & "Phonebook.csv")
            End If


            For x As Integer = 0 To DgvPersonal.Rows.Count - 1
                CmbNumber.Items.Add(MyPhoneBook(x).FirstName & " " & MyPhoneBook(x).Surname)
            Next x

            Me.Text = "SPA Call Manager Pro - " & MyStoredPhoneSettings.StationName & " - " & MyStoredPhoneSettings.PhoneModel & "-" & MyStoredPhoneSettings.PhoneSoftwareVersion
        Else
            MsgBox("No ping response from handset IP (" & MyStoredPhoneSettings.PhoneIP & ") - Failed to load data from handset")
            LoadPhoneBook(dataDir & "\CiscoPhone\Phonebook.csv")
            If SharedDataDir <> "" Then
                FSW.Path = SharedDataDir
                FrmSetup.TxtSharedFolder.Text = SharedDataDir
                LoadSharedPhoneBook(SharedDataDir & "Phonebook.csv")
            End If

            For x As Integer = 0 To DgvPersonal.Rows.Count - 1
                CmbNumber.Items.Add(MyPhoneBook(x).FirstName & " " & MyPhoneBook(x).Surname)
            Next x
        End If

        Me.SPAToolTips.SetToolTip(Me.CmbNumber, "Type a number directly into this field and press return to dial," & vbCrLf & "or search the currently selected directory by typing a contacts name.")
        Me.SPAToolTips.SetToolTip(Me.BtnDial1, "Click to dial on this line.  Click while on a call to place the call on hold")
        Me.SPAToolTips.SetToolTip(Me.BtnDial2, "Click to dial on this line.  Click while on a call to place the call on hold")
        Me.SPAToolTips.SetToolTip(Me.BtnDial3, "Click to dial on this line.  Click while on a call to place the call on hold")
        Me.SPAToolTips.SetToolTip(Me.BtnDial4, "Click to dial on this line.  Click while on a call to place the call on hold")

    End Sub

    Private Sub FrmMain_FormClosing(ByVal sender As Object, ByVal e As System.Windows.Forms.FormClosingEventArgs) Handles Me.FormClosing

        NF1.Visible = False 'removes the notify icon

    End Sub

    Private Sub FrmMain_Paint(ByVal sender As Object, ByVal e As System.Windows.Forms.PaintEventArgs) Handles Me.Paint
        'paints form with slight gradient
        On Error Resume Next
        If sender.width = 0 Then Exit Sub
        If sender.height = 0 Then Exit Sub

        Dim a As New LinearGradientBrush(New RectangleF(0, 0, Me.Width, Me.Height), Color.SlateGray, Color.Black, LinearGradientMode.Vertical)
        Dim gg As Graphics = e.Graphics
        gg.FillRectangle(a, New RectangleF(0, 0, Me.Width, Me.Height))
        a = Nothing
        gg = Nothing


    End Sub

    Private Sub BtnAddPhoneEntry_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnAddPhoneEntry.Click

        'opens form to add new phonebook entry
        Dim NewFrmPhonebook As New FrmPhoneBook("", "", "", -1, "DgvPersonal")
        NewFrmPhonebook.ShowDialog()
        CmbNumber.Items.Clear()
        For x As Integer = 0 To DgvPersonal.Rows.Count - 1
            CmbNumber.Items.Add(MyPhoneBook(x).FirstName & " " & MyPhoneBook(x).Surname)
        Next x

    End Sub

    Private Sub MyPhone_UDPRxdata(ByVal PhoneStatusdata As ClsPhone.sPhoneStatus) Handles MyPhone.UDPRxdata
        'data passed from myphone object....data from phone activity
        On Error GoTo UDPRXError
        PhoneStatusdata.CallerName = ""
        Dim tempNumber As String = LinePhoneStatus(PhoneStatusdata.Id).CallerNumber

        LinePhoneStatus(PhoneStatusdata.Id) = PhoneStatusdata
        If LinePhoneStatus(PhoneStatusdata.Id).CallerNumber = "" Then LinePhoneStatus(PhoneStatusdata.Id).CallerNumber = tempNumber
        If LinePhoneStatus(PhoneStatusdata.Id).Status = ClsPhone.ePhoneStatus.Dialing Then LinePhoneStatus(PhoneStatusdata.Id).CallerNumber = ""


        For x As Integer = 0 To MyPhoneBook.GetUpperBound(0) - 1
            If LinePhoneStatus(PhoneStatusdata.Id).CallerNumber = MyPhoneBook(x).Number Then
                LinePhoneStatus(PhoneStatusdata.Id).CallerName = MyPhoneBook(x).FirstName & " " & MyPhoneBook(x).Surname
                PhoneStatusdata.CallerName = LinePhoneStatus(PhoneStatusdata.Id).CallerName
            End If
        Next

        For x As Integer = 0 To DGVPhoneDir.Rows.Count - 1
            If LinePhoneStatus(PhoneStatusdata.Id).CallerNumber = DGVPhoneDir.Rows(x).Cells(2).Value Then
                LinePhoneStatus(PhoneStatusdata.Id).CallerName = DGVPhoneDir.Rows(x).Cells(1).Value
                PhoneStatusdata.CallerName = LinePhoneStatus(PhoneStatusdata.Id).CallerName
            End If
        Next

        Select Case PhoneStatusdata.Status
            Case ClsPhone.ePhoneStatus.Ringing
                MyPhoneStatus = PhoneStatusdata
                Select Case MyPhoneStatus.Id
                    Case "1"
                        LblLine1.Text = "Ringing " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial1.Image = IlButtons.Images(0)
                        newFrmCallLine1 = New FrmCall(MyPhoneStatus)
                        newFrmCallLine1.Show()
                        newFrmCallLine1.TopMost = True
                        newFrmCallLine1.Left = SystemInformation.WorkingArea.Width - newFrmCallLine1.Width
                        newFrmCallLine1.Top = SystemInformation.WorkingArea.Height - newFrmCallLine1.Height
                        If newFrmCallLine2 IsNot Nothing Then
                            newFrmCallLine2.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 2)
                        End If
                        BtnHang1.Enabled = True
                    Case "2"
                        LblLine2.Text = "Ringing " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial2.Image = IlButtons.Images(0)
                        newFrmCallLine2 = New FrmCall(MyPhoneStatus)
                        newFrmCallLine2.Show()
                        newFrmCallLine2.TopMost = True
                        newFrmCallLine2.Left = SystemInformation.WorkingArea.Width - newFrmCallLine2.Width
                        newFrmCallLine2.Top = SystemInformation.WorkingArea.Height - newFrmCallLine2.Height
                        If newFrmCallLine1 IsNot Nothing = True Then
                            newFrmCallLine2.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 2)
                        End If
                        BtnHang2.Enabled = True
                    Case "3"
                        LblLine3.Text = "Ringing " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial3.Image = IlButtons.Images(0)
                        newFrmCallLine3 = New FrmCall(MyPhoneStatus)
                        newFrmCallLine3.Show()
                        newFrmCallLine3.TopMost = True
                        newFrmCallLine3.Left = SystemInformation.WorkingArea.Width - newFrmCallLine3.Width
                        If newFrmCallLine1 IsNot Nothing And newFrmCallLine2 IsNot Nothing Then newFrmCallLine3.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 3)
                        If newFrmCallLine1 Is Nothing And newFrmCallLine2 IsNot Nothing Then newFrmCallLine3.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 2)
                        If newFrmCallLine1 IsNot Nothing And newFrmCallLine2 Is Nothing Then newFrmCallLine3.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 2)
                        If newFrmCallLine1 Is Nothing And newFrmCallLine2 Is Nothing Then newFrmCallLine3.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height)
                        BtnHang3.Enabled = True
                    Case "4"
                        LblLine4.Text = "Ringing " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial4.Image = IlButtons.Images(0)
                        newFrmCallLine4 = New FrmCall(MyPhoneStatus)
                        newFrmCallLine4.Show()
                        newFrmCallLine4.TopMost = True
                        newFrmCallLine4.Left = SystemInformation.WorkingArea.Width - newFrmCallLine4.Width
                        If newFrmCallLine1 IsNot Nothing And newFrmCallLine2 IsNot Nothing And newFrmCallLine3 IsNot Nothing Then newFrmCallLine4.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 4)
                        If newFrmCallLine1 IsNot Nothing And newFrmCallLine2 IsNot Nothing And newFrmCallLine3 Is Nothing Then newFrmCallLine4.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 3)
                        If newFrmCallLine1 IsNot Nothing And newFrmCallLine2 Is Nothing And newFrmCallLine3 IsNot Nothing Then newFrmCallLine4.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 3)
                        If newFrmCallLine1 IsNot Nothing And newFrmCallLine2 Is Nothing And newFrmCallLine3 Is Nothing Then newFrmCallLine4.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 2)
                        If newFrmCallLine1 Is Nothing And newFrmCallLine2 IsNot Nothing And newFrmCallLine3 IsNot Nothing Then newFrmCallLine4.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 3)
                        If newFrmCallLine1 Is Nothing And newFrmCallLine2 IsNot Nothing And newFrmCallLine3 Is Nothing Then newFrmCallLine4.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 2)
                        If newFrmCallLine1 Is Nothing And newFrmCallLine2 Is Nothing And newFrmCallLine3 IsNot Nothing Then newFrmCallLine4.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 2)
                        If newFrmCallLine1 Is Nothing And newFrmCallLine2 Is Nothing And newFrmCallLine3 Is Nothing Then newFrmCallLine4.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height)
                        BtnHang4.Enabled = True
                End Select
            Case ClsPhone.ePhoneStatus.Connected
                Select Case PhoneStatusdata.Id
                    Case 1
                        LblLine1.Text = "Connected " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial1.Image = IlButtons.Images(0)
                        BtnHang1.Enabled = True
                        HoldFlash(1) = False
                    Case 2
                        LblLine2.Text = "Connected " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial2.Image = IlButtons.Images(0)
                        BtnHang2.Enabled = True
                        HoldFlash(2) = False
                    Case 3
                        LblLine3.Text = "Connected " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial3.Image = IlButtons.Images(0)
                        BtnHang3.Enabled = True
                        HoldFlash(3) = False
                    Case 4
                        LblLine4.Text = "Connected " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial4.Image = IlButtons.Images(0)
                        BtnHang4.Enabled = True
                        HoldFlash(4) = False
                End Select
                FrmFade(PhoneStatusdata.Id) = True
            Case ClsPhone.ePhoneStatus.Dialing
                Select Case PhoneStatusdata.Id
                    Case 1
                        LblLine1.Text = "Off hook " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial1.Image = IlButtons.Images(0)
                        BtnHang1.Enabled = True
                    Case 2
                        LblLine2.Text = "Off hook " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial2.Image = IlButtons.Images(0)
                        BtnHang2.Enabled = True
                    Case 3
                        LblLine3.Text = "Off hook " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial3.Image = IlButtons.Images(0)
                        BtnHang3.Enabled = True
                    Case 4
                        LblLine4.Text = "Off hook " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial4.Image = IlButtons.Images(0)
                        BtnHang4.Enabled = True
                End Select
            Case ClsPhone.ePhoneStatus.Calling
                Select Case PhoneStatusdata.Id
                    Case 1
                        LblLine1.Text = "Calling " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial1.Image = IlButtons.Images(0)
                        BtnHang1.Enabled = True
                    Case 2
                        LblLine2.Text = "Calling " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial2.Image = IlButtons.Images(0)
                        BtnHang2.Enabled = True
                    Case 3
                        LblLine3.Text = "Calling " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial3.Image = IlButtons.Images(0)
                        BtnHang3.Enabled = True
                    Case 4
                        LblLine4.Text = "Calling " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial4.Image = IlButtons.Images(0)
                        BtnHang4.Enabled = True
                End Select
            Case ClsPhone.ePhoneStatus.Holding, ClsPhone.ePhoneStatus.Hold
                Select Case PhoneStatusdata.Id
                    Case 1
                        LblLine1.Text = "Holding " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial1.Image = IlButtons.Images(3)
                        HoldFlash(1) = True
                        BtnHang1.Enabled = True
                    Case 2
                        LblLine2.Text = "Holding " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial2.Image = IlButtons.Images(3)
                        BtnHang2.Enabled = True
                        HoldFlash(2) = True
                    Case 3
                        LblLine3.Text = "Holding " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial3.Image = IlButtons.Images(3)
                        BtnHang3.Enabled = True
                        HoldFlash(3) = True
                    Case 4
                        LblLine4.Text = "Holding " & LinePhoneStatus(PhoneStatusdata.Id).CallerName
                        BtnDial4.Image = IlButtons.Images(3)
                        BtnHang4.Enabled = True
                        HoldFlash(4) = True
                End Select


            Case ClsPhone.ePhoneStatus.Idle
                Select Case PhoneStatusdata.Id
                    Case 1
                        LblLine1.Text = "Line 1"
                        BtnHang1.Enabled = False
                        BtnDial1.Image = IlButtons.Images(2)
                        HoldFlash(1) = False
                    Case 2
                        LblLine2.Text = "Line 2"
                        BtnHang2.Enabled = False
                        BtnDial2.Image = IlButtons.Images(2)
                        HoldFlash(2) = False
                    Case 3
                        LblLine3.Text = "Line 3"
                        BtnHang3.Enabled = False
                        BtnDial3.Image = IlButtons.Images(2)
                        HoldFlash(3) = False
                    Case 4
                        LblLine4.Text = "Line 4"
                        BtnHang4.Enabled = False
                        BtnDial4.Image = IlButtons.Images(2)
                        HoldFlash(4) = False
                End Select
                FrmFade(PhoneStatusdata.Id) = True
        End Select


        Exit Sub

UDPRXError:

        ' MsgBox(Err.Description)
        Resume Next

    End Sub

    Private Sub TmrFadeNotification_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TmrFadeNotification.Tick

        'fades notification form when closed

        On Error Resume Next

        For x As Integer = 1 To 4
            If FrmFade(x) = True Then
                Select Case x
                    Case 1
                        newFrmCallLine1.Opacity = newFrmCallLine1.Opacity - 0.05
                        If newFrmCallLine1.Opacity <= 0 Then
                            newFrmCallLine1.Visible = False
                            newFrmCallLine1 = Nothing
                            FrmFade(x) = False
                        End If
                    Case 2
                        newFrmCallLine2.Opacity = newFrmCallLine2.Opacity - 0.05
                        If newFrmCallLine2.Opacity <= 0 Then
                            newFrmCallLine2.Visible = False
                            newFrmCallLine2 = Nothing
                            FrmFade(x) = False
                        End If
                    Case 3
                        newFrmCallLine3.Opacity = newFrmCallLine3.Opacity - 0.05
                        If newFrmCallLine3.Opacity <= 0 Then
                            newFrmCallLine3.Visible = False
                            newFrmCallLine3 = Nothing
                            FrmFade(x) = False
                        End If
                    Case 4
                        newFrmCallLine4.Opacity = newFrmCallLine4.Opacity - 0.05
                        If newFrmCallLine4.Opacity <= 0 Then
                            newFrmCallLine4.Visible = False
                            newFrmCallLine4 = Nothing
                            FrmFade(x) = False
                        End If
                End Select
            End If
        Next


    End Sub

    Public Sub GetPhoneDir(ByVal URL As String)
        'Function to download the entire personal address book from the phone handset into an array.  This is used to autofill the combobox for dialing numbers

        Try
            Dim client As WebClient = New WebClient()
            Dim BookName As String = ""
            Dim BookNumber As String = ""
            Dim FindElement As Integer = 0
            Dim Counter As Integer = 0
            Dim dirdata As IO.Stream = client.OpenRead(URL)
            Dim reader As StreamReader = New StreamReader(dirdata)
            Dim strdata As String = reader.ReadToEnd

            'Dim tr As IO.TextReader = New IO.StreamReader("C:\audio\Phonedir.html")

            'strdata = tr.ReadToEnd


            DGVPhoneDir.Rows.Clear()

            Do
                FindElement = strdata.IndexOf("<input class=""input", FindElement + 1)
                If FindElement = -1 Then Exit Do
                Dim Number As String = strdata.Substring(strdata.IndexOf("value=", FindElement) + 7, strdata.IndexOf(Chr(34), strdata.IndexOf("value=", FindElement) + 7) - (strdata.IndexOf("value=", FindElement) + 7))
                If Number.StartsWith("n=") Then
                    Dim tempdata() As String = Number.Split(";")
                    For f As Integer = 0 To tempdata.GetUpperBound(0)
                        Select Case tempdata(f).Substring(0, 1)
                            Case "n"
                                BookName = tempdata(f).Substring(2)
                            Case "p"
                                BookNumber = tempdata(f).Substring(2)
                            Case "p"
                        End Select
                    Next
                    If BookNumber <> "" Then
                        DGVPhoneDir.Rows.Add()
                        DGVPhoneDir.Rows(Counter).Cells(3).Value = "Call"
                        DGVPhoneDir.Rows(Counter).Cells(1).Value = BookName
                        DGVPhoneDir.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(BookNumber)
                        DGVPhoneDir.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    End If
                Else
                    If Number <> "" Then
                        DGVPhoneDir.Rows.Add()
                        DGVPhoneDir.Rows(Counter).Cells(3).Value = "Call"
                        If Number.StartsWith("9") Then Number = Number.Substring(1)
                        DGVPhoneDir.Rows(Counter).Cells(1).Value = "" '"Unknown " & Counter.ToString
                        DGVPhoneDir.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number)
                        DGVPhoneDir.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    End If
                End If
            Loop

        Catch ex As Exception

        End Try

    End Sub

    Public Sub GetPhoneCalled(ByVal URL As String)

        Try
            Dim oldFiletype As Boolean = False
            Dim client As WebClient = New WebClient()
            Dim FindElement As Integer = 0
            Dim Counter As Integer = 0
            Dim dirdata As IO.Stream = client.OpenRead(URL)
            Dim reader As StreamReader = New StreamReader(dirdata)
            Dim strdata As String = reader.ReadToEnd

            'Dim tr As IO.TextReader = New IO.StreamReader("C:\audio\calllog.htm")
            'strdata = tr.ReadToEnd


            FindElement = strdata.IndexOf("<div class=""tab-page"" id=""Placed"">", FindElement + 1)
            If FindElement = -1 Then
                FindElement = strdata.IndexOf("<div class=""tab-page"" id=""Redial List"">", FindElement + 1)
                oldFiletype = True
            End If
            DGWdialled.Rows.Clear()
            Do
                FindElement = strdata.IndexOf("<td>&nbsp;", FindElement + 1)
                If FindElement = -1 Then Exit Do
                Dim Number() As String = strdata.Substring(FindElement + 10, strdata.IndexOf("<", FindElement + 10) - (FindElement + 10)).Split(",")
                DGWdialled.Rows.Add()
                DGWdialled.Rows(Counter).Cells(3).Value = "Call"
                If oldFiletype = True Then
                    If Number.GetUpperBound(0) = 2 Then
                        If Number(1).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWdialled.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWdialled.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(1))
                        DGWdialled.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    Else
                        If Number(0).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWdialled.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWdialled.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWdialled.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    End If
                Else
                    If Number.GetUpperBound(0) = 3 Then
                        If Number(1).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWdialled.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWdialled.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(1))
                        DGWdialled.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    Else
                        If Number(0).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWdialled.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWdialled.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWdialled.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    End If


                End If

          
            Loop Until Counter = 60

        Catch ex As Exception

        End Try

    End Sub

    Public Sub GetPhoneAnswered(ByVal URL As String)

        'retrieves and parses html retrieved form phone for anserwed calls

        Try
            Dim oldFiletype As Boolean = False
            Dim client As WebClient = New WebClient()
            Dim FindElement As Integer = 0
            Dim Counter As Integer = 0
            Dim dirdata As IO.Stream = client.OpenRead(URL)
            Dim reader As StreamReader = New StreamReader(dirdata)
            Dim strdata As String = reader.ReadToEnd

            'Dim tr As IO.TextReader = New IO.StreamReader("C:\audio\calllog.htm")
            'strdata = tr.ReadToEnd


            FindElement = strdata.IndexOf("<div class=""tab-page"" id=""Answered"">", FindElement + 1)
            If FindElement = -1 Then
                FindElement = strdata.IndexOf("<div class=""tab-page"" id=""Answered Calls"">", FindElement + 1)
                oldFiletype = True
            End If

            DGWAnswered.Rows.Clear()
            Do
                FindElement = strdata.IndexOf("<td>&nbsp;", FindElement + 1)
                If FindElement = -1 Then Exit Do
                Dim Number() As String = strdata.Substring(FindElement + 10, strdata.IndexOf("<", FindElement + 10) - (FindElement + 10)).Split(",")
                DGWAnswered.Rows.Add()
                DGWAnswered.Rows(Counter).Cells(3).Value = "Call"
                If oldFiletype = True Then
                    If Number.GetUpperBound(0) = 2 Then
                        If Number(1).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWAnswered.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWAnswered.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(1))
                        DGWAnswered.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    Else
                        If Number(0).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWAnswered.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWAnswered.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWAnswered.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    End If
                Else
                    If Number.GetUpperBound(0) = 3 Then
                        If Number(1).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWAnswered.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWAnswered.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(1))
                        DGWAnswered.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    Else
                        If Number(0).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWAnswered.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWAnswered.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWAnswered.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    End If

                End If
            Loop Until Counter = 60
        Catch ex As Exception

        End Try

    End Sub

    Public Sub GetPhoneMissed(ByVal URL As String)
        'retrieves and parses html retrieved form phone for missed calls
        Try
            Dim oldFiletype As Boolean = False
            Dim client As WebClient = New WebClient()
            Dim FindElement As Integer = 0
            Dim Counter As Integer = 0
            Dim dirdata As IO.Stream = client.OpenRead(URL)
            Dim reader As StreamReader = New StreamReader(dirdata)
            Dim strdata As String = reader.ReadToEnd

            'Dim tr As IO.TextReader = New IO.StreamReader("C:\audio\calllog.htm")
            'strdata = tr.ReadToEnd

            FindElement = strdata.IndexOf("<div class=""tab-page"" id=""Missed"">", FindElement + 1)
            If FindElement = -1 Then
                FindElement = strdata.IndexOf("<div class=""tab-page"" id=""Missed Calls"">", FindElement + 1)
                oldFiletype = True
            End If

            DGWMissed.Rows.Clear()
            Do
                FindElement = strdata.IndexOf("<td>&nbsp;", FindElement + 1)
                If FindElement = -1 Then Exit Do
                Dim Number() As String = strdata.Substring(FindElement + 10, strdata.IndexOf("<", FindElement + 10) - (FindElement + 10)).Split(",")
                DGWMissed.Rows.Add()
                DGWMissed.Rows(Counter).Cells(3).Value = "Call"
                If oldFiletype = True Then
                    If Number.GetUpperBound(0) = 2 Then
                        If Number(1).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWMissed.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWMissed.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(1))
                        DGWMissed.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    Else
                        If Number(0).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWMissed.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWMissed.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWMissed.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    End If
                Else
                    If Number.GetUpperBound(0) = 3 Then
                        If Number(1).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWMissed.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWMissed.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(1))
                        DGWMissed.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    Else
                        If Number(0).StartsWith("9") Then Number(0) = Number(0).Substring(1)
                        DGWMissed.Rows(Counter).Cells(1).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWMissed.Rows(Counter).Cells(2).Value = RemoveLineDetailsFromNumber(Number(0))
                        DGWMissed.Rows(Counter).Cells(0).Value = Counter + 1
                        Counter = Counter + 1
                    End If
                End If
            Loop Until Counter = 60
        Catch ex As Exception

        End Try


    End Sub

    Private Sub DGWAnswered_CellContentClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DGWAnswered.CellContentClick

        'calls the number in the grid row, when the call button is clicked 

        If sender.CurrentCell.ColumnIndex = 3 Then
            Dim result As Integer = FindFreeLine() ' finds a free line...ie so if line i is in use it will chosse lone 2 to call out on.
            If result = 0 Then Exit Sub
            Dim MycallControl As New CallControl
            LinePhoneStatus(result).Id = result
            LinePhoneStatus(result).CallerNumber = sender.Item(2, sender.CurrentCell.RowIndex).Value
            CmbNumber.Text = LinePhoneStatus(result).CallerNumber
            Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Dial, LinePhoneStatus(result), MyPhoneSettings)
            MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort) ' sends data to phone to initiate call
            MycallControl = Nothing
        End If

    End Sub

    Private Sub DGWdialled_CellContentClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DGWdialled.CellContentClick

        'calls the number in the grid row, when the call button is clicked 
        If sender.CurrentCell.ColumnIndex = 3 Then
            Dim result As Integer = FindFreeLine() ' finds a free line...ie so if line i is in use it will chosse lone 2 to call out on.
            If result = 0 Then Exit Sub
            Dim MycallControl As New CallControl
            LinePhoneStatus(result).Id = result
            LinePhoneStatus(result).CallerNumber = sender.Item(2, sender.CurrentCell.RowIndex).Value
            CmbNumber.Text = LinePhoneStatus(result).CallerNumber
            Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Dial, LinePhoneStatus(result), MyPhoneSettings)
            MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort) ' sends data to phone to initiate call
            MycallControl = Nothing
        End If

    End Sub

    Private Sub DGWMissed_CellContentClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DGWMissed.CellContentClick

        'calls the number in the grid row, when the call button is clicked 
        If sender.CurrentCell.ColumnIndex = 3 Then
            Dim result As Integer = FindFreeLine() ' finds a free line...ie so if line i is in use it will chosse lone 2 to call out on.
            If result = 0 Then Exit Sub
            Dim MycallControl As New CallControl
            LinePhoneStatus(result).Id = result
            LinePhoneStatus(result).CallerNumber = sender.Item(2, sender.CurrentCell.RowIndex).Value
            CmbNumber.Text = LinePhoneStatus(result).CallerNumber
            Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Dial, LinePhoneStatus(result), MyPhoneSettings)
            MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort) ' sends data to phone to initiate call
            MycallControl = Nothing
        End If

    End Sub

    Private Sub DgvPersonal_CellContentClick(ByVal sender As Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DgvPersonal.CellContentClick

        'calls the number in the grid row, when the call button is clicked 
        If sender.CurrentCell.ColumnIndex = 3 Then
            Dim result As Integer = FindFreeLine() ' finds a free line...ie so if line i is in use it will chosse lone 2 to call out on.
            If result = 0 Then Exit Sub
            Dim MycallControl As New CallControl
            LinePhoneStatus(result).Id = result
            LinePhoneStatus(result).CallerNumber = sender.Item(2, sender.CurrentCell.RowIndex).Value
            LinePhoneStatus(result).CallerName = sender.Item(1, sender.CurrentCell.RowIndex).Value
            CmbNumber.Text = LinePhoneStatus(result).CallerName
            Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Dial, LinePhoneStatus(result), MyPhoneSettings)
            MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort) ' sends data to phone to initiate call
            MycallControl = Nothing
        End If


    End Sub

    Private Sub DGVPhoneDir_CellContentClick(ByVal sender As System.Object, ByVal e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DGVPhoneDir.CellContentClick

        'calls the number in the grid row, when the call button is clicked 
        If sender.CurrentCell.ColumnIndex = 3 Then
            Dim result As Integer = FindFreeLine() ' finds a free line...ie so if line i is in use it will chosse lone 2 to call out on.
            If result = 0 Then Exit Sub
            Dim MycallControl As New CallControl
            LinePhoneStatus(result).Id = result
            LinePhoneStatus(result).CallerNumber = sender.Item(2, sender.CurrentCell.RowIndex).Value
            LinePhoneStatus(result).CallerName = sender.Item(1, sender.CurrentCell.RowIndex).Value
            If LinePhoneStatus(result).CallerName = "" Then
                CmbNumber.Text = LinePhoneStatus(result).CallerNumber
            Else
                CmbNumber.Text = LinePhoneStatus(result).CallerName
            End If
            Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Dial, LinePhoneStatus(result), MyPhoneSettings)
            MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort) ' sends data to phone to initiate call
            MycallControl = Nothing
        End If


    End Sub

    Private Sub DgvPersonal_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles DgvPersonal.DoubleClick

        'gets the number in the grid row  and opens up the form to edit the dtails 
        If sender.CurrentCell.ColumnIndex <> 3 Then
            Dim TmpName() As String = sender.Item(1, sender.CurrentCell.RowIndex).Value.split(" ")
            Dim NewFrmPhonebook As New FrmPhoneBook(TmpName(0), TmpName(1), sender.Item(2, sender.CurrentCell.RowIndex).Value, sender.CurrentCell.RowIndex, sender.name)
            NewFrmPhonebook.ShowDialog()
        End If

    End Sub

    Private Sub DgvPersonal_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles DgvPersonal.KeyDown

        ' Deletes the entry in the selected row by hitting the delete key

        If e.KeyData = Keys.Delete Then
            Dim result As MsgBoxResult = MsgBox("Do you wish to delete" & vbCrLf & sender.Item(1, sender.CurrentCell.RowIndex).Value & "?", MsgBoxStyle.YesNo Or MsgBoxStyle.Critical, "Phone Book")
            If result = MsgBoxResult.Yes Then
                'removes entry from the myphonebook array
                MyPhoneBook(sender.CurrentCell.RowIndex).FirstName = ""
                MyPhoneBook(sender.CurrentCell.RowIndex).Surname = ""
                MyPhoneBook(sender.CurrentCell.RowIndex).Number = ""
                SavePhoneBook(dataDir & "\CiscoPhone\Phonebook.csv")
                LoadPhoneBook(dataDir & "\CiscoPhone\Phonebook.csv")
            End If
        End If

    End Sub

    Private Sub FrmMain_Resize(ByVal sender As Object, ByVal e As System.EventArgs) Handles Me.Resize

        If Me.WindowState = FormWindowState.Minimized Then Me.Visible = False

        Me.Width = 629

    End Sub

    Private Sub clpbrd_ClipBoardItemAdded(ByVal data As String) Handles clpbrd.ClipBoardItemAdded

        'called when a new clipboard item is saved ie in the copy command....and checks if its a number before adding to the dial number box
        data = data.Replace(" ", "")
        data = data.Replace("(", "")
        data = data.Replace(")", "")
        data = data.Replace("-", "")

        Dim IsNum As Boolean = IsNumeric(data)
        If IsNum = True Then CmbNumber.Text = data

    End Sub

    Private Sub BtnDial_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnDial1.Click, BtnDial2.Click, BtnDial3.Click, BtnDial4.Click

        'Called when any of the line buttons are clicked...
        Dim NumberToCall As String = CmbNumber.Text
        NumberToCall = NumberToCall.Replace(" ", "")
        NumberToCall = NumberToCall.Replace("(", "")
        NumberToCall = NumberToCall.Replace(")", "")
        NumberToCall = NumberToCall.Replace("-", "")

        Dim MycallControl As New CallControl

        Select Case sender.name
            Case "BtnDial1"
                MyPhoneStatus.Id = 1
            Case "BtnDial2"
                MyPhoneStatus.Id = 2
            Case "BtnDial3"
                MyPhoneStatus.Id = 3
            Case "BtnDial4"
                MyPhoneStatus.Id = 4
        End Select

        Select Case LinePhoneStatus(MyPhoneStatus.Id).Status
            Case ClsPhone.ePhoneStatus.Answering

            Case ClsPhone.ePhoneStatus.Calling

            Case ClsPhone.ePhoneStatus.Connected
                ' If the line is connected then put on hold
                Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Hold, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
                MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
                MycallControl = Nothing

            Case ClsPhone.ePhoneStatus.Dialing
            Case ClsPhone.ePhoneStatus.Holding
                ' If the line is on hold  then take off hold
                Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Resume, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
                MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
                MycallControl = Nothing


            Case ClsPhone.ePhoneStatus.Idle
                ' If the line is idfle then dial number in number box
                If NumberToCall <> "" Then
                    If IsNumeric(NumberToCall) = True Then
                        LinePhoneStatus(MyPhoneStatus.Id).CallerNumber = NumberToCall
                    Else
                        If CmbNumber.SelectedIndex > -1 Then
                            If TbDirectories.SelectedIndex = 0 Then
                                LinePhoneStatus(MyPhoneStatus.Id).CallerNumber = MyPhoneBook(CmbNumber.SelectedIndex).Number
                            ElseIf TbDirectories.SelectedIndex = 1 Then
                                LinePhoneStatus(MyPhoneStatus.Id).CallerNumber = DGVSharedDir.Rows(CmbNumber.SelectedIndex).Cells(2).Value
                            ElseIf TbDirectories.SelectedIndex = 2 Then
                                LinePhoneStatus(MyPhoneStatus.Id).CallerNumber = DGVPhoneDir.Rows(CmbNumber.SelectedIndex).Cells(2).Value
                                Exit Sub
                            End If
                        End If
                    End If
                    LinePhoneStatus(MyPhoneStatus.Id).Id = MyPhoneStatus.Id
                    Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Dial, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
                    MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
                    MycallControl = Nothing
                End If
            Case ClsPhone.ePhoneStatus.Ringing
                ' If the line is ringing then answer
                Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Answer, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
                MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
                MycallControl = Nothing

        End Select


    End Sub

    Private Sub BtnHang1_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnHang1.Click, BtnHang2.Click, BtnHang3.Click, BtnHang4.Click

        'Called when any of the hangup buttons are clicked...

        Dim MycallControl As New CallControl

        Select Case sender.name
            Case "BtnDial1"
                MyPhoneStatus.Id = 1
            Case "BtnDial2"
                MyPhoneStatus.Id = 2
            Case "BtnDial3"
                MyPhoneStatus.Id = 3
            Case "BtnDial4"
                MyPhoneStatus.Id = 4
        End Select

        'hangs up the call
        Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.End, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
        MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
        MycallControl = Nothing



    End Sub

    Private Sub BtnSetup_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles BtnSetup.Click

        'populates and shows the setup dialog

        'FrmSetup.LblstationName.Text = "Station Name: " & MyPhoneSettings.StationName
        FrmSetup.LbldebugAddress.Text = "Phone debug address: " & MyPhoneSettings.Debug_Server_Address
        FrmSetup.LblLocalIp.Text = "Local IP Address"
        FrmSetup.CmbLocalIP.Items.AddRange(MyPhone.GetLocalIp)
        FrmSetup.CmbLocalIP.Text = MyPhoneSettings.LocalIP
        FrmSetup.LblPhoneIp.Text = "Phone IP Address"
        FrmSetup.TxtphoneIP.Text = MyPhoneSettings.PhoneIP
        FrmSetup.txtpassword.Text = MyPhone.password
        FrmSetup.ShowDialog()

    End Sub

    Public Function FindFreeLine() As Integer

        ' checks through the linestatus objects for the fisrt free line....if all are in use returns 0 and no action is taken
        For x As Integer = 1 To 4
            If LinePhoneStatus(x).Status = ClsPhone.ePhoneStatus.Idle Then
                Return x
                Exit Function
            End If
        Next

        Return 0

    End Function

    Private Sub ShowToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ShowToolStripMenuItem.Click

        'context menu item when right clikcing on the notify icon....this will show the form if minimized
        If Me.WindowState = FormWindowState.Minimized Then
            Me.Visible = True
            Me.WindowState = FormWindowState.Normal
        End If

    End Sub

    Private Sub ExitToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles ExitToolStripMenuItem.Click

        'context menu item when right clikcing on the notify icon....this will cloase the application
        Me.Close()

    End Sub

    Private Sub TmrFlash_Tick(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles TmrFlash.Tick

        index = IIf(index = 0, 3, 0)

        For x As Integer = 1 To 4
            If HoldFlash(x) = True Then
                Select Case x
                    Case 1
                        BtnDial1.Image = IlButtons.Images(index)
                    Case 2
                        BtnDial2.Image = IlButtons.Images(index)
                    Case 3
                        BtnDial3.Image = IlButtons.Images(index)
                    Case 4
                        BtnDial4.Image = IlButtons.Images(index)
                End Select
            End If
        Next

    End Sub

    Private Sub CmbNumber_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles CmbNumber.KeyDown

        If e.KeyData = Keys.Enter Then
            DialFromSpeedDialBox()
        End If


    End Sub

    Private Sub DialFromSpeedDialBox()
        Dim NumberToCall As String = CmbNumber.Text
        NumberToCall = NumberToCall.Replace(" ", "")
        NumberToCall = NumberToCall.Replace("(", "")
        NumberToCall = NumberToCall.Replace(")", "")
        NumberToCall = NumberToCall.Replace("-", "")

        Dim MycallControl As New CallControl
        Dim result As Integer = FindFreeLine() ' finds a free line...ie so if line i is in use it will chosse lone 2 to call out on.
        If result = 0 Then Exit Sub
        If NumberToCall <> "" Then
            If IsNumeric(NumberToCall) = True Then
                LinePhoneStatus(MyPhoneStatus.Id).CallerNumber = NumberToCall
            Else
                If CmbNumber.SelectedIndex > -1 Then
                    If TbDirectories.SelectedIndex = 0 Then
                        MyPhoneBook(CmbNumber.SelectedIndex).Number = MyPhoneBook(CmbNumber.SelectedIndex).Number.Replace(" ", "")
                        MyPhoneBook(CmbNumber.SelectedIndex).Number = MyPhoneBook(CmbNumber.SelectedIndex).Number.Replace("(", "")
                        MyPhoneBook(CmbNumber.SelectedIndex).Number = MyPhoneBook(CmbNumber.SelectedIndex).Number.Replace(")", "")
                        MyPhoneBook(CmbNumber.SelectedIndex).Number = MyPhoneBook(CmbNumber.SelectedIndex).Number.Replace("-", "")
                        LinePhoneStatus(MyPhoneStatus.Id).CallerNumber = MyPhoneBook(CmbNumber.SelectedIndex).Number
                    Else
                        Dim TmpNumber As String = DGVPhoneDir.Rows(CmbNumber.SelectedIndex).Cells(2).Value
                        TmpNumber = TmpNumber.Replace(" ", "")
                        TmpNumber = TmpNumber.Replace("(", "")
                        TmpNumber = TmpNumber.Replace(")", "")
                        TmpNumber = TmpNumber.Replace("-", "")
                        LinePhoneStatus(MyPhoneStatus.Id).CallerNumber = TmpNumber
                    End If
                Else
                    Exit Sub
                End If
            End If
            LinePhoneStatus(MyPhoneStatus.Id).Id = MyPhoneStatus.Id
            Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Dial, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
            MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
            MycallControl = Nothing
        End If
    End Sub

    Private Sub DGVPhoneDir_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles DGVPhoneDir.DoubleClick

        If sender.CurrentCell.ColumnIndex <> 3 Then
            Dim NewFrmPhonebook As New FrmPhoneBook(sender.Item(1, sender.CurrentCell.RowIndex).Value, "", sender.Item(2, sender.CurrentCell.RowIndex).Value, sender.CurrentCell.RowIndex, sender.name)
            NewFrmPhonebook.ShowDialog()
        End If

    End Sub

    Private Sub DGWAnswered_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles DGWAnswered.DoubleClick

        If sender.CurrentCell.ColumnIndex <> 3 Then
            Dim NewFrmPhonebook As New FrmPhoneBook(sender.Item(1, sender.CurrentCell.RowIndex).Value, "", sender.Item(2, sender.CurrentCell.RowIndex).Value, sender.CurrentCell.RowIndex, sender.name)
            NewFrmPhonebook.ShowDialog()
        End If

    End Sub

    Private Sub DGWMissed_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles DGWMissed.DoubleClick

        If sender.CurrentCell.ColumnIndex <> 3 Then
            Dim NewFrmPhonebook As New FrmPhoneBook(sender.Item(1, sender.CurrentCell.RowIndex).Value, "", sender.Item(2, sender.CurrentCell.RowIndex).Value, sender.CurrentCell.RowIndex, sender.name)
            NewFrmPhonebook.ShowDialog()
        End If

    End Sub

    Private Sub DGWdialled_DoubleClick(ByVal sender As Object, ByVal e As System.EventArgs) Handles DGWdialled.DoubleClick

        If sender.CurrentCell.ColumnIndex <> 3 Then
            Dim NewFrmPhonebook As New FrmPhoneBook(sender.Item(1, sender.CurrentCell.RowIndex).Value, "", sender.Item(2, sender.CurrentCell.RowIndex).Value, sender.CurrentCell.RowIndex, sender.name)
            NewFrmPhonebook.ShowDialog()
        End If

    End Sub


    Private Sub btnQuickDial_Click(sender As System.Object, e As System.EventArgs)
        DialFromSpeedDialBox()
    End Sub

    Public Sub CheckForExistingInstance()
        'Get number of processes of you program
        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then

            MessageBox.Show _
             ("SPA Call Manager Pro is already running.  This instance will now close", _
                 "SPA Call Manager Pro", _
                  MessageBoxButtons.OK, _
                 MessageBoxIcon.Exclamation)
            Application.Exit()
        End If
    End Sub

   
    Private Sub TbDirectories_Click(sender As Object, e As System.EventArgs) Handles TbDirectories.Click

        ' refreshes data from the phone when the tab is clicked so that all data is up to date

        Select Case TbDirectories.SelectedIndex
            Case 0
                CmbNumber.Items.Clear()
                For x As Integer = 0 To DgvPersonal.Rows.Count - 1
                    CmbNumber.Items.Add(MyPhoneBook(x).FirstName & " " & MyPhoneBook(x).Surname)
                Next x
            Case 1
                CmbNumber.Items.Clear()
                For x As Integer = 0 To DGVSharedDir.Rows.Count - 1
                    CmbNumber.Items.Add(DGVSharedDir.Rows(x).Cells(1).Value)
                Next x
            Case 2
                If My.Computer.Network.Ping(MyStoredPhoneSettings.PhoneIP) Then
                    GetPhoneDir("http://" & MyStoredPhoneSettings.PhoneIP & "/pdir.htm")
                    CmbNumber.Items.Clear()
                    For x As Integer = 0 To DGVPhoneDir.Rows.Count - 1
                        If DGVPhoneDir.Rows(x).Cells(2).Value <> "" Then
                            If DGVPhoneDir.Rows(x).Cells(1).Value <> "" Then
                                CmbNumber.Items.Add(DGVPhoneDir.Rows(x).Cells(1).Value)
                            Else
                                CmbNumber.Items.Add(DGVPhoneDir.Rows(x).Cells(2).Value)
                            End If
                        End If
                    Next x
                End If
            Case 3
                If My.Computer.Network.Ping(MyStoredPhoneSettings.PhoneIP) Then
                    GetPhoneCalled("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")
                End If
            Case 4
                If My.Computer.Network.Ping(MyStoredPhoneSettings.PhoneIP) Then
                    GetPhoneAnswered("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")
                End If
            Case 5
                If My.Computer.Network.Ping(MyStoredPhoneSettings.PhoneIP) Then
                    GetPhoneMissed("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")
                End If
        End Select


    End Sub

    Private Sub DGVSharedDir_CellContentClick(sender As Object, e As System.Windows.Forms.DataGridViewCellEventArgs) Handles DGVSharedDir.CellContentClick

        If sender.CurrentCell.ColumnIndex = 3 Then
            Dim result As Integer = FindFreeLine() ' finds a free line...ie so if line i is in use it will chosse lone 2 to call out on.
            If result = 0 Then Exit Sub
            Dim MycallControl As New CallControl
            LinePhoneStatus(result).Id = result
            LinePhoneStatus(result).CallerNumber = sender.Item(2, sender.CurrentCell.RowIndex).Value
            LinePhoneStatus(result).CallerName = sender.Item(1, sender.CurrentCell.RowIndex).Value
            CmbNumber.Text = LinePhoneStatus(result).CallerName
            Dim CallString As String = MycallControl.PhoneAction(CallControl.eAction.Dial, LinePhoneStatus(result), MyPhoneSettings)
            MyPhone.SendUdp(CallString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort) ' sends data to phone to initiate call
            MycallControl = Nothing
        End If

    End Sub

    Private Sub DGVSharedDir_DoubleClick(sender As Object, e As System.EventArgs) Handles DGVSharedDir.DoubleClick


        If sender.CurrentCell.ColumnIndex <> 3 Then
            Dim item = sender.Item(1, sender.CurrentCell.RowIndex)
            Dim itemValue = item.Value
            Dim TmpName() As String = itemValue.split(" ")
            Dim NewFrmPhonebook As New FrmPhoneBook(TmpName(0), TmpName(1), sender.Item(2, sender.CurrentCell.RowIndex).Value, sender.CurrentCell.RowIndex, sender.name)
            NewFrmPhonebook.ShowDialog()
        End If

    End Sub

    Private Sub DGVSharedDir_KeyDown(sender As Object, e As System.Windows.Forms.KeyEventArgs) Handles DGVSharedDir.KeyDown


        If e.KeyData = Keys.Delete Then
            Dim result As MsgBoxResult = MsgBox("Do you wish to delete" & vbCrLf & sender.Item(1, sender.CurrentCell.RowIndex).Value & "?", MsgBoxStyle.YesNo Or MsgBoxStyle.Critical, "Phone Book")
            If result = MsgBoxResult.Yes Then
                'removes entry from the myphonebook array
                MySharedPhoneBook(sender.CurrentCell.RowIndex).FirstName = ""
                MySharedPhoneBook(sender.CurrentCell.RowIndex).Surname = ""
                MySharedPhoneBook(sender.CurrentCell.RowIndex).Number = ""
                SaveSharedPhoneBook(SharedDataDir & "Phonebook.csv")
                LoadSharedPhoneBook(SharedDataDir & "Phonebook.csv")
            End If
        End If

    End Sub

    Private Sub FSW_Changed(sender As Object, e As System.IO.FileSystemEventArgs) Handles FSW.Changed


        LoadSharedPhoneBook(SharedDataDir & "Phonebook.csv")

    End Sub
End Class
