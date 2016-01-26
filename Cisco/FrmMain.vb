Imports System.IO
Imports System.Net
Imports Cisco.Utilities
Imports Cisco.Utilities.ClsPhone
Imports Pss.Cisco.Models

Public Class FrmMain

    Dim newFrmCallLine1 As FrmCall 'popup form for line1
    Dim newFrmCallLine2 As FrmCall 'popup form for line2
    Dim newFrmCallLine3 As FrmCall 'popup form for line3
    Dim newFrmCallLine4 As FrmCall 'popup form for line4

    Private ReadOnly FrmFade(4) As Boolean ''Sets if fade is enabled when form closes
    Private ReadOnly HoldFlash(4) As Boolean ''Sets if fade is enabled when form closes
    Private Shared ReadOnly LinePhoneStatus(4) As SPhoneStatus ' status of each line object
    Public WithEvents clpbrd As New ClipBoardMonitor ' monitors the clipboard for telephone numbers

    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.

        DgvPersonal.AutoGenerateColumns = False
        DgvPersonal.DataSource = MyPhoneBook

        DGVSharedDir.AutoGenerateColumns = False
        DGVSharedDir.DataSource = MySharedPhoneBook

        DGVPhoneDir.AutoGenerateColumns = False
        DGVPhoneDir.DataSource = PhoneDir

        DGWMissed.AutoGenerateColumns = False
        DGWMissed.DataSource = Missed

        DGWdialled.AutoGenerateColumns = False
        DGWdialled.DataSource = Dialled

        DGWAnswered.AutoGenerateColumns = False
        DGWAnswered.DataSource = Answered

        AddHandler UdpRxdata, AddressOf MyPhone_UDPRxdata

    End Sub

    Private Sub FrmMain_Load(sender As Object, e As EventArgs) Handles MyBase.Load

        CheckForExistingInstance()

        ' sets the app data folder and creates one if there isnt one already
        DataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)
        If Directory.Exists(DataDir & "\CiscoPhone") = False Then
            Directory.CreateDirectory(DataDir & "\CiscoPhone")
        End If

        'Get stored settings for the phone and local Ip address of the PC
        MyStoredPhoneSettings = GetStoredSettings()

        'Me.Text = "SPA Call Manager Pro - " & MyStoredPhoneSettings.StationName & " - " & MyStoredPhoneSettings.PhoneModel & "-" & MyStoredPhoneSettings.PhoneSoftwareVersion

        If MyStoredPhoneSettings.PhoneIP = "" Then
            'If the Phone IP Address has not yet been populated, first show the setup dialog with some default values and then retreive the values for use afterwards.

            FrmSetup.LblLocalIp.Text = "Local IP Address"
            FrmSetup.CmbLocalIP.Items.AddRange(GetLocalIp())
            FrmSetup.CmbLocalIP.SelectedIndex = 0
            FrmSetup.LblPhoneIp.Text = "Phone IP Address"
            If FrmSetup.ShowDialog() = DialogResult.Cancel Then
                Me.Close()
            End If

            'Now that we have shown the form dialog, retreive the settings for use by the program
            MyStoredPhoneSettings = GetStoredSettings()
        End If

        'Try and ping the IP address of the phone before trying to communicate with it as this just casues a long delay waiting for things to timeout.
        If String.IsNullOrEmpty(MyStoredPhoneSettings.PhoneIP) Then
            MsgBox("IP address of handset has not been set up")
        Else
            Dim pingError As Exception = Nothing
            Dim pingSuccess As Boolean = NetUtils.PingHandset(MyStoredPhoneSettings.PhoneIP, pingError)

            If Not pingSuccess Then
                If pingError Is Nothing Then
                    MsgBox("No ping response from handset IP (" & MyStoredPhoneSettings.PhoneIP & ") - Failed to load data from handset")
                Else
                    MsgBox("An error ocurred while attempting to reach the handset on ip address (" & MyStoredPhoneSettings.PhoneIP & ").")
                End If
            End If

            If pingSuccess Then
                'download phone settings
                MyPhoneSettings.password = MyStoredPhoneSettings.password
                Password = MyPhoneSettings.password
                LoginPassword = MyPhoneSettings.password

                Dim warning = String.Empty
                MyPhoneSettings = DownloadPhoneSettings(MyStoredPhoneSettings.PhoneIP, MyStoredPhoneSettings.LocalIP, warning)

                If (Not String.IsNullOrWhiteSpace(warning)) Then
                    MsgBox(warning, MsgBoxStyle.Critical, "SPA Call Manager Pro")
                End If

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

                IpPort = MyStoredPhoneSettings.LocalPort
                Startlistening()

                TmrFadeNotification.Enabled = True
                For x = 1 To 4
                    FrmFade(x) = False
                Next

                'retrieve call data from phone
                GetPhoneDir("http://" & MyStoredPhoneSettings.PhoneIP & "/pdir.htm")
                GetPhoneCalled("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")
                GetPhoneAnswered("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")
                GetPhoneMissed("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")

                Me.Text = String.Format("SPA Call Manager Pro - {0} - {1}-{2}",
                                        MyStoredPhoneSettings.StationName,
                                        MyStoredPhoneSettings.PhoneModel,
                                        MyStoredPhoneSettings.PhoneSoftwareVersion)
            End If
        End If

        InitializePhonebooks()

        Me.SPAToolTips.SetToolTip(Me.CmbNumber, "Type a number directly into this field and press return to dial," & vbCrLf & "or search the currently selected directory by typing a contacts name.")
        Me.SPAToolTips.SetToolTip(Me.BtnDial1, "Click to dial on this line.  Click while on a call to place the call on hold")
        Me.SPAToolTips.SetToolTip(Me.BtnDial2, "Click to dial on this line.  Click while on a call to place the call on hold")
        Me.SPAToolTips.SetToolTip(Me.BtnDial3, "Click to dial on this line.  Click while on a call to place the call on hold")
        Me.SPAToolTips.SetToolTip(Me.BtnDial4, "Click to dial on this line.  Click while on a call to place the call on hold")

    End Sub

    Private Sub InitializePhonebooks()
        LoadPhoneBook(Path.Combine(DataDir, "CiscoPhone\Phonebook.csv"))

        If MyStoredPhoneSettings.sharedDataDir <> "" Then
            FSW.Path = MyStoredPhoneSettings.sharedDataDir
            FrmSetup.TxtSharedFolder.Text = MyStoredPhoneSettings.sharedDataDir
            LoadSharedPhoneBook(Path.Combine(MyStoredPhoneSettings.sharedDataDir, "Phonebook.csv"))
        End If

        RefillCombinedPhonebook()

    End Sub

    Private Sub RefillCombinedPhonebook()
        CombinedPhoneBook.Clear()
        CombinedPhoneBook.AddRange(
            MyPhoneBook.Union(MySharedPhoneBook) _
                       .Union(PhoneDir) _
                       .Union(Missed) _
                       .Union(Dialled) _
                       .Union(Answered).Distinct().OrderBy(Function(x) x.DisplayName))

        CmbNumber.Items.Clear()
        CmbNumber.Items.AddRange(CombinedPhoneBook.ToArray())
    End Sub

    Private Sub FrmMain_FormClosing(sender As Object, e As FormClosingEventArgs) Handles Me.FormClosing

        NF1.Visible = False 'removes the notify icon

    End Sub

    Private Sub FrmMain_Paint(sender As Object, e As PaintEventArgs) Handles Me.Paint
        'paints form with slight gradient
        Me.PaintGradient(e.Graphics)
    End Sub

    Private Sub BtnAddPhoneEntry_Click(sender As Object, e As EventArgs) Handles BtnAddPhoneEntry.Click

        'opens form to add new phonebook entry
        Dim newFrmPhonebook As New FrmPhoneBook(Nothing, -1, "DgvPersonal")
        newFrmPhonebook.ShowDialog()

        RefillCombinedPhonebook()

    End Sub

    Private Sub MyPhone_UDPRxdata(phoneStatusdata As SPhoneStatus)
        'data passed from myphone object....data from phone activity

        Try

            phoneStatusdata.CallerName = ""
            Dim phoneStatus As SPhoneStatus = LinePhoneStatus(phoneStatusdata.Id)
            Dim tempNumber As String = phoneStatus.CallerNumber

            LinePhoneStatus(phoneStatusdata.Id) = phoneStatusdata

            If String.IsNullOrWhiteSpace(phoneStatus.CallerNumber) Then phoneStatus.CallerNumber = tempNumber
            If phoneStatus.Status = EPhoneStatus.Dialing Then phoneStatus.CallerNumber = ""

            For Each entry In MyPhoneBook
                If String.Equals(phoneStatus.CallerNumber, entry.Number) Then
                    phoneStatus.CallerName = entry.DisplayName
                    phoneStatusdata.CallerName = phoneStatus.CallerName
                End If
            Next

            For Each entry In PhoneDir
                If String.Equals(phoneStatus.CallerNumber, entry.Number) Then
                    phoneStatus.CallerName = entry.DisplayName
                    phoneStatusdata.CallerName = entry.DisplayName
                End If
            Next

            Select Case phoneStatusdata.Status
                Case EPhoneStatus.Ringing
                    MyPhoneStatus = phoneStatusdata
                    Select Case MyPhoneStatus.Id
                        Case 1
                            LblLine1.Text = "Ringing " & phoneStatus.CallerName
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
                        Case 2
                            LblLine2.Text = "Ringing " & phoneStatus.CallerName
                            BtnDial2.Image = IlButtons.Images(0)
                            newFrmCallLine2 = New FrmCall(MyPhoneStatus)
                            newFrmCallLine2.Show()
                            newFrmCallLine2.TopMost = True
                            newFrmCallLine2.Left = SystemInformation.WorkingArea.Width - newFrmCallLine2.Width
                            newFrmCallLine2.Top = SystemInformation.WorkingArea.Height - newFrmCallLine2.Height
                            If (newFrmCallLine1 IsNot Nothing) Then
                                newFrmCallLine2.Top = SystemInformation.WorkingArea.Height - (newFrmCallLine2.Height * 2)
                            End If
                            BtnHang2.Enabled = True
                        Case 3
                            LblLine3.Text = "Ringing " & phoneStatus.CallerName
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
                        Case 4
                            LblLine4.Text = "Ringing " & phoneStatus.CallerName
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
                Case EPhoneStatus.Connected
                    Select Case phoneStatusdata.Id
                        Case 1
                            LblLine1.Text = "Connected " & phoneStatus.CallerName
                            BtnDial1.Image = IlButtons.Images(0)
                            BtnHang1.Enabled = True
                            HoldFlash(1) = False
                        Case 2
                            LblLine2.Text = "Connected " & phoneStatus.CallerName
                            BtnDial2.Image = IlButtons.Images(0)
                            BtnHang2.Enabled = True
                            HoldFlash(2) = False
                        Case 3
                            LblLine3.Text = "Connected " & phoneStatus.CallerName
                            BtnDial3.Image = IlButtons.Images(0)
                            BtnHang3.Enabled = True
                            HoldFlash(3) = False
                        Case 4
                            LblLine4.Text = "Connected " & phoneStatus.CallerName
                            BtnDial4.Image = IlButtons.Images(0)
                            BtnHang4.Enabled = True
                            HoldFlash(4) = False
                    End Select
                    FrmFade(phoneStatusdata.Id) = True
                Case EPhoneStatus.Dialing
                    Select Case phoneStatusdata.Id
                        Case 1
                            LblLine1.Text = "Off hook " & phoneStatus.CallerName
                            BtnDial1.Image = IlButtons.Images(0)
                            BtnHang1.Enabled = True
                        Case 2
                            LblLine2.Text = "Off hook " & phoneStatus.CallerName
                            BtnDial2.Image = IlButtons.Images(0)
                            BtnHang2.Enabled = True
                        Case 3
                            LblLine3.Text = "Off hook " & phoneStatus.CallerName
                            BtnDial3.Image = IlButtons.Images(0)
                            BtnHang3.Enabled = True
                        Case 4
                            LblLine4.Text = "Off hook " & phoneStatus.CallerName
                            BtnDial4.Image = IlButtons.Images(0)
                            BtnHang4.Enabled = True
                    End Select
                Case EPhoneStatus.Calling
                    Select Case phoneStatusdata.Id
                        Case 1
                            LblLine1.Text = "Calling " & phoneStatus.CallerName
                            BtnDial1.Image = IlButtons.Images(0)
                            BtnHang1.Enabled = True
                        Case 2
                            LblLine2.Text = "Calling " & phoneStatus.CallerName
                            BtnDial2.Image = IlButtons.Images(0)
                            BtnHang2.Enabled = True
                        Case 3
                            LblLine3.Text = "Calling " & phoneStatus.CallerName
                            BtnDial3.Image = IlButtons.Images(0)
                            BtnHang3.Enabled = True
                        Case 4
                            LblLine4.Text = "Calling " & phoneStatus.CallerName
                            BtnDial4.Image = IlButtons.Images(0)
                            BtnHang4.Enabled = True
                    End Select
                Case EPhoneStatus.Holding, EPhoneStatus.Hold
                    Select Case phoneStatusdata.Id
                        Case 1
                            LblLine1.Text = "Holding " & phoneStatus.CallerName
                            BtnDial1.Image = IlButtons.Images(3)
                            HoldFlash(1) = True
                            BtnHang1.Enabled = True
                        Case 2
                            LblLine2.Text = "Holding " & phoneStatus.CallerName
                            BtnDial2.Image = IlButtons.Images(3)
                            BtnHang2.Enabled = True
                            HoldFlash(2) = True
                        Case 3
                            LblLine3.Text = "Holding " & phoneStatus.CallerName
                            BtnDial3.Image = IlButtons.Images(3)
                            BtnHang3.Enabled = True
                            HoldFlash(3) = True
                        Case 4
                            LblLine4.Text = "Holding " & phoneStatus.CallerName
                            BtnDial4.Image = IlButtons.Images(3)
                            BtnHang4.Enabled = True
                            HoldFlash(4) = True
                    End Select


                Case EPhoneStatus.Idle
                    Select Case phoneStatusdata.Id
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
                    FrmFade(phoneStatusdata.Id) = True

                Case EPhoneStatus.Unknown
                    'TODO:This should not be logged as exception, but may be logged as warning
                Case Else
                    Dim argumentEx As New ArgumentOutOfRangeException(
                                            "phoneStatusdata.Status",
                                            phoneStatusdata.Status,
                                            String.Format("Phone status '{0}' was not handled for handling phone status data : {1}",
                                                          phoneStatusdata.Status,
                                                          phoneStatusdata.ToString()))
                    argumentEx.Log()
            End Select

        Catch ex As Exception
            ex.Log()
        End Try

    End Sub

    Private Sub TmrFadeNotification_Tick(sender As Object, e As EventArgs) Handles TmrFadeNotification.Tick

        'fades notification form when closed
        Try
            For x = 1 To 4
                If FrmFade(x) = True Then
                    Select Case x
                        Case 1
                            If newFrmCallLine1 Is Nothing Then Continue For

                            newFrmCallLine1.Opacity -= 0.05
                            If newFrmCallLine1.Opacity <= 0 Then
                                newFrmCallLine1.Visible = False
                                newFrmCallLine1 = Nothing
                                FrmFade(x) = False
                            End If
                        Case 2
                            If newFrmCallLine2 Is Nothing Then Continue For

                            newFrmCallLine2.Opacity -= 0.05
                            If newFrmCallLine2.Opacity <= 0 Then
                                newFrmCallLine2.Visible = False
                                newFrmCallLine2 = Nothing
                                FrmFade(x) = False
                            End If
                        Case 3
                            If newFrmCallLine3 Is Nothing Then Continue For

                            newFrmCallLine3.Opacity -= 0.05
                            If newFrmCallLine3.Opacity <= 0 Then
                                newFrmCallLine3.Visible = False
                                newFrmCallLine3 = Nothing
                                FrmFade(x) = False
                            End If
                        Case 4
                            If newFrmCallLine4 Is Nothing Then Continue For

                            newFrmCallLine4.Opacity -= 0.05
                            If newFrmCallLine4.Opacity <= 0 Then
                                newFrmCallLine4.Visible = False
                                newFrmCallLine4 = Nothing
                                FrmFade(x) = False
                            End If
                    End Select
                End If
            Next

        Catch ex As Exception
            ex.Log()
        End Try
    End Sub

    Public Sub GetPhoneDir(URL As String)
        'Function to download the entire personal address book from the phone handset into an array. This is used to autofill the combobox for dialing numbers

        Try
            Dim strdata As String

            Using client As New WebClient()
                Dim dirdata = client.OpenRead(URL)
                Using reader = New StreamReader(dirdata)
                    strdata = reader.ReadToEnd
                End Using

            End Using

            PhoneDir.Clear()

            Dim findElement = 0

            Do While PhoneDir.Count < 1000

                Dim bookName = ""
                Dim bookNumber = ""

                findElement = strdata.IndexOf("<input class=""input", findElement + 1, StringComparison.InvariantCultureIgnoreCase)
                If findElement = -1 Then Exit Do

                Dim number As String = strdata.Substring(strdata.IndexOf("value=", findElement, StringComparison.InvariantCultureIgnoreCase) + 7, strdata.IndexOf(Chr(34), strdata.IndexOf("value=", findElement, StringComparison.InvariantCultureIgnoreCase) + 7) - (strdata.IndexOf("value=", findElement, StringComparison.InvariantCultureIgnoreCase) + 7))
                If number.StartsWith("n=") Then
                    Dim tempdata() As String = number.Split(";".ToCharArray())
                    For f = 0 To tempdata.GetUpperBound(0)
                        Select Case tempdata(f).Substring(0, 1)
                            Case "n"
                                bookName = tempdata(f).Substring(2)
                            Case "p"
                                bookNumber = tempdata(f).Substring(2)
                            Case "p"
                        End Select
                    Next
                    If bookNumber <> "" Then
                        Dim entry As New PhoneBookEntry()
                        With entry
                            .FirstName = bookName
                            .Number = RemoveLineDetailsFromNumber(bookNumber)
                        End With
                        PhoneDir.Add(entry)

                    End If
                Else
                    If number <> "" Then

                        If number.StartsWith("9") Then number = number.Substring(1)

                        Dim entry As New PhoneBookEntry()
                        With entry
                            .Number = RemoveLineDetailsFromNumber(number)
                        End With
                        PhoneDir.Add(entry)

                    End If
                End If
            Loop

        Catch ex As Exception
            ex.Log()
        End Try

    End Sub

    Public Sub GetPhoneCalled(URL As String)

        GetPhoneEntries(URL, "Placed", "Redial List", Dialled)

    End Sub

    Public Sub GetPhoneAnswered(URL As String)

        'retrieves and parses html retrieved form phone for anserwed calls
        GetPhoneEntries(URL, "Answered", "Answered Calls", Answered)

    End Sub

    Private Shared Sub GetPhoneEntries(url As String, tableId As String, oldTableId As String, entryList As IList(Of PhoneBookEntry))
        Try
            Dim strdata As String

            Using client = New WebClient()
                Using dirdata = client.OpenRead(url)
                    Dim reader = New StreamReader(dirdata)
                    strdata = reader.ReadToEnd
                End Using
            End Using

            Dim oldFiletype = False

            Dim findElement = strdata.IndexOf(String.Format("<div class=""tab-page"" id=""{0}"">", tableId))

            If findElement = -1 Then
                findElement = strdata.IndexOf(String.Format("<div class=""tab-page"" id=""{0}"">", oldTableId), findElement + 1)
                oldFiletype = True
            End If

            entryList.Clear()

            'TODO: Parse list using :
            '  <td>&nbsp;(?<name>[^,\n<]+),(?<Number>[^,\n<]+),(?<Time>[^,\n<]+)<

            While entryList.Count < 1000

                findElement = strdata.IndexOf("<td>&nbsp;", findElement + 1)
                If findElement = -1 Then Exit While
                Dim number() As String = strdata.Substring(findElement + 10, strdata.IndexOf("<", findElement + 10) - (findElement + 10)).Split(",".ToCharArray())

                Dim entry = New PhoneBookEntry()

                If oldFiletype = True Then
                    If number.GetUpperBound(0) = 2 Then
                        If number(1).StartsWith("9") Then number(0) = number(0).Substring(1)

                        entry.FirstName = RemoveLineDetailsFromNumber(number(0))
                        entry.Number = RemoveLineDetailsFromNumber(number(1))
                    Else
                        If number(0).StartsWith("9") Then number(0) = number(0).Substring(1)

                        entry.FirstName = RemoveLineDetailsFromNumber(number(0))
                        entry.Number = RemoveLineDetailsFromNumber(number(0))

                    End If
                Else
                    If number.GetUpperBound(0) = 3 Then
                        If number(1).StartsWith("9") Then number(0) = number(0).Substring(1)

                        entry.FirstName = RemoveLineDetailsFromNumber(number(0))
                        entry.Number = RemoveLineDetailsFromNumber(number(1))

                    Else
                        If number(0).StartsWith("9") Then number(0) = number(0).Substring(1)
                        entry.FirstName = RemoveLineDetailsFromNumber(number(0))
                        entry.Number = RemoveLineDetailsFromNumber(number(0))

                    End If

                End If

                entryList.Add(entry)

            End While


        Catch ex As Exception
            ex.Log()
        End Try
    End Sub

    Public Sub GetPhoneMissed(url As String)
        GetPhoneEntries(url, "Missed", "Missed Calls", Missed)
    End Sub
    Private Sub CallPhoneBookEntry(entry As PhoneBookEntry)

        Dim result = FindFreeLine() ' finds a free line...ie so if line i is in use it will chosse lone 2 to call out on.
        If result = 0 Then
            MsgBox(String.Format("Could not obtain a free line. The status of your lines is : {0}", String.Join(", ", LinePhoneStatus.Select(Function(x) x.Status))))
            Return
        End If

        LinePhoneStatus(result).Id = result
        LinePhoneStatus(result).CallerNumber = entry.Number
        LinePhoneStatus(result).CallerName = entry.DisplayName

        Dim callString As String = PhoneAction(EAction.Dial, LinePhoneStatus(result), MyPhoneSettings)
        SendUdp(callString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort) ' sends data to phone to initiate call

        CmbNumber.SelectedItem = entry
    End Sub

    Private Sub DGWAnswered_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DGWAnswered.CellContentClick
        If (CType(sender, DataGridView).CurrentCell Is Nothing) Then Return
        'calls the number in the grid row, when the call button is clicked 

        If TypeOf (CType(sender, DataGridView).Columns(e.ColumnIndex)) Is DataGridViewButtonColumn Then
            Dim entry = Answered(e.RowIndex)
            CallPhoneBookEntry(entry)
        End If

    End Sub



    Private Sub DGWdialled_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DGWdialled.CellContentClick
        If (CType(sender, DataGridView).CurrentCell Is Nothing) Then Return

        'calls the number in the grid row, when the call button is clicked 
        If TypeOf (CType(sender, DataGridView).Columns(e.ColumnIndex)) Is DataGridViewButtonColumn Then
            Dim entry = Dialled(DGWdialled.CurrentCell.RowIndex)
            CallPhoneBookEntry(entry)
        End If

    End Sub

    Private Sub DGWMissed_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DGWMissed.CellContentClick
        If (CType(sender, DataGridView).CurrentCell Is Nothing) Then Return

        'calls the number in the grid row, when the call button is clicked 
        If TypeOf (CType(sender, DataGridView).Columns(e.ColumnIndex)) Is DataGridViewButtonColumn Then
            Dim entry = Missed(DGWMissed.CurrentCell.RowIndex)
            CallPhoneBookEntry(entry)
        End If

    End Sub

    Private Sub DgvPersonal_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DgvPersonal.CellContentClick
        If (CType(sender, DataGridView).CurrentCell Is Nothing) Then Return

        'calls the number in the grid row, when the call button is clicked 
        If TypeOf (CType(sender, DataGridView).Columns(e.ColumnIndex)) Is DataGridViewButtonColumn Then
            Dim entry = MyPhoneBook(DgvPersonal.CurrentCell.RowIndex)
            CallPhoneBookEntry(entry)
        End If

    End Sub

    Private Sub DGVPhoneDir_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DGVPhoneDir.CellContentClick
        If (CType(sender, DataGridView).CurrentCell Is Nothing) Then Return

        'calls the number in the grid row, when the call button is clicked 
        If TypeOf (CType(sender, DataGridView).Columns(e.ColumnIndex)) Is DataGridViewButtonColumn Then
            Dim entry = PhoneDir(DgvPersonal.CurrentCell.RowIndex)
            CallPhoneBookEntry(entry)
        End If

    End Sub

    Private Sub DgvPersonal_DoubleClick(sender As Object, e As EventArgs) Handles DgvPersonal.DoubleClick
        If (CType(sender, DataGridView).CurrentCell Is Nothing) Then Return

        If Not TypeOf (CType(sender, DataGridView).CurrentCell.OwningColumn) Is DataGridViewButtonColumn Then
            Dim entry = MyPhoneBook(DgvPersonal.CurrentCell.RowIndex)

            Dim newFrmPhonebook As New FrmPhoneBook(entry, DgvPersonal.CurrentCell.RowIndex, DgvPersonal.Name)
            newFrmPhonebook.ShowDialog()
        End If

    End Sub

    Private Sub DgvPersonal_KeyDown(sender As Object, e As KeyEventArgs) Handles DgvPersonal.KeyDown
        If (CType(sender, DataGridView).CurrentCell Is Nothing) Then Return

        ' Deletes the entry in the selected row by hitting the delete key

        If e.KeyData = Keys.Delete Then
            Dim entry = MyPhoneBook(DgvPersonal.CurrentCell.RowIndex)
            Dim result As MsgBoxResult = MsgBox("Do you wish to delete" & vbCrLf & entry.DisplayName & "?", MsgBoxStyle.YesNo Or MsgBoxStyle.Critical, "Phone Book")
            If result = MsgBoxResult.Yes Then
                'removes entry from the myphonebook array
                MyPhoneBook.RemoveAt(DgvPersonal.CurrentCell.RowIndex)
                SavePhoneBook(Path.Combine(DataDir, "CiscoPhone\Phonebook.csv"))
                LoadPhoneBook(Path.Combine(DataDir, "CiscoPhone\Phonebook.csv"))
            End If
        End If

    End Sub

    Private Sub FrmMain_Resize(sender As Object, e As EventArgs) Handles Me.Resize

        If Me.WindowState = FormWindowState.Minimized Then Me.Visible = False

        Me.Width = 629

    End Sub

    Private Sub clpbrd_ClipBoardItemAdded(data As String) Handles clpbrd.ClipBoardItemAdded

        'called when a new clipboard item is saved ie in the copy command....and checks if its a number before adding to the dial number box
        data = data.Replace(" ", "") _
                   .Replace("(", "") _
                   .Replace(")", "") _
                   .Replace("-", "")

        If IsNumeric(data) Then CmbNumber.Text = data

    End Sub

    Private Function GetNumberFromSpeedDialBox() As String

        Dim result As String

        If TypeOf (CmbNumber.SelectedItem) Is PhoneBookEntry Then
            result = CType(CmbNumber.SelectedItem, PhoneBookEntry).Number
        Else
            result = CmbNumber.Text.Replace(" ", "") _
                                   .Replace("(", "") _
                                   .Replace(")", "") _
                                   .Replace("-", "")
        End If

        Return result
    End Function

    Private Sub BtnDial_Click(sender As Object, e As EventArgs) Handles BtnDial1.Click, BtnDial2.Click, BtnDial3.Click, BtnDial4.Click

        'Called when any of the line buttons are clicked...

        Dim numberToCall = GetNumberFromSpeedDialBox()

        Select Case CType(sender, Control).Name
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
            Case EPhoneStatus.Answering

            Case EPhoneStatus.Calling

            Case EPhoneStatus.Connected
                ' If the line is connected then put on hold
                Dim callString As String = PhoneAction(EAction.Hold, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
                SendUdp(callString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)


            Case EPhoneStatus.Dialing
            Case EPhoneStatus.Holding
                ' If the line is on hold  then take off hold
                Dim callString As String = PhoneAction(EAction.Resume, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
                SendUdp(callString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)

            Case EPhoneStatus.Idle
                ' If the line is idle then dial number in number box
                If numberToCall <> "" Then
                    If IsNumeric(numberToCall) = True Then
                        LinePhoneStatus(MyPhoneStatus.Id).CallerNumber = numberToCall
                    Else
                        If CmbNumber.SelectedIndex > -1 Then
                            LinePhoneStatus(MyPhoneStatus.Id).CallerNumber = CombinedPhoneBook(CmbNumber.SelectedIndex).Number
                        End If
                    End If
                    LinePhoneStatus(MyPhoneStatus.Id).Id = MyPhoneStatus.Id
                    Dim callString As String = PhoneAction(EAction.Dial, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
                    SendUdp(callString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)
                End If
            Case EPhoneStatus.Ringing
                ' If the line is ringing then answer
                Dim callString As String = PhoneAction(EAction.Answer, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
                SendUdp(callString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)

        End Select


    End Sub

    Private Sub BtnHang1_Click(sender As Object, e As EventArgs) Handles BtnHang1.Click, BtnHang2.Click, BtnHang3.Click, BtnHang4.Click

        'Called when any of the hangup buttons are clicked...

        Select Case CType(sender, Control).Name
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
        Dim callString As String = PhoneAction(EAction.End, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
        SendUdp(callString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)


    End Sub

    Private Sub BtnSetup_Click(sender As Object, e As EventArgs) Handles BtnSetup.Click

        'populates and shows the setup dialog

        'FrmSetup.LblstationName.Text = "Station Name: " & MyPhoneSettings.StationName
        FrmSetup.LbldebugAddress.Text = "Phone debug address: " & MyPhoneSettings.Debug_Server_Address
        FrmSetup.LblLocalIp.Text = "Local IP Address"
        FrmSetup.CmbLocalIP.Items.AddRange(GetLocalIp())
        FrmSetup.CmbLocalIP.Text = MyPhoneSettings.LocalIP
        FrmSetup.LblPhoneIp.Text = "Phone IP Address"
        FrmSetup.TxtphoneIP.Text = MyPhoneSettings.PhoneIP
        FrmSetup.txtpassword.Text = Password
        FrmSetup.ShowDialog()

    End Sub

    Public Shared Function FindFreeLine() As Integer

        ' checks through the linestatus objects for the fisrt free line....if all are in use returns 0 and no action is taken
        For x = 1 To 4
            If LinePhoneStatus(x).Status = EPhoneStatus.Idle Then
                Return x
            End If
        Next

        Return 0

    End Function

    Private Sub ShowToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ShowToolStripMenuItem.Click

        'context menu item when right clikcing on the notify icon....this will show the form if minimized
        If Me.WindowState = FormWindowState.Minimized Then
            Me.Visible = True
            Me.WindowState = FormWindowState.Normal
        End If

    End Sub

    Private Sub ExitToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles ExitToolStripMenuItem.Click

        'context menu item when right clikcing on the notify icon....this will cloase the application
        Me.Close()

    End Sub

    ''' <summary>
    ''' Very simple way of keeping track flash status of dialing image
    ''' </summary>
    Private flashIndex As Integer = 0
    Private Sub TmrFlash_Tick(sender As Object, e As EventArgs) Handles TmrFlash.Tick

        flashIndex = If(flashIndex = 0, 3, 0)

        For x = 1 To 4
            If HoldFlash(x) = True Then
                Select Case x
                    Case 1
                        BtnDial1.Image = IlButtons.Images(flashIndex)
                    Case 2
                        BtnDial2.Image = IlButtons.Images(flashIndex)
                    Case 3
                        BtnDial3.Image = IlButtons.Images(flashIndex)
                    Case 4
                        BtnDial4.Image = IlButtons.Images(flashIndex)
                End Select
            End If
        Next

    End Sub

    Private Sub CmbNumber_KeyDown(sender As Object, e As KeyEventArgs) Handles CmbNumber.KeyDown

        If e.KeyData = Keys.Enter Then
            DialFromSpeedDialBox()
        End If


    End Sub

    Private Sub DialFromSpeedDialBox()
        Dim NumberToCall = GetNumberFromSpeedDialBox()


        Dim result As Integer = FindFreeLine() ' finds a free line...ie so if line i is in use it will chosse lone 2 to call out on.
        If result = 0 Then
            MsgBox("No line available")
            Exit Sub
        End If

        If NumberToCall <> "" Then
            If IsNumeric(NumberToCall) = True Then
                LinePhoneStatus(MyPhoneStatus.Id).CallerNumber = NumberToCall
            Else
                MsgBox(String.Format("'{0}' is not a valid number for calling.k", NumberToCall))
                Exit Sub
            End If
            LinePhoneStatus(MyPhoneStatus.Id).Id = MyPhoneStatus.Id
            Dim callString As String = PhoneAction(EAction.Dial, LinePhoneStatus(MyPhoneStatus.Id), MyPhoneSettings)
            SendUdp(callString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort)

        End If
    End Sub

    Private Sub DGVPhoneDir_DoubleClick(sender As Object, e As EventArgs) Handles DGVPhoneDir.DoubleClick
        EditPhoneEntry(DGVPhoneDir, PhoneDir)
    End Sub


    Private Sub EditPhoneEntry(dataGridView As DataGridView, entries As IList(Of PhoneBookEntry))

        If dataGridView.CurrentCell Is Nothing Then Return

        If Not TypeOf (dataGridView.CurrentCell.OwningColumn) Is DataGridViewButtonColumn Then
            Dim entry = entries(dataGridView.CurrentCell.RowIndex)

            Dim newFrmPhonebook As New FrmPhoneBook(
                entry,
                dataGridView.CurrentCell.RowIndex,
                dataGridView.Name)

            newFrmPhonebook.ShowDialog()
        End If

    End Sub

    Private Sub DGWAnswered_DoubleClick(sender As Object, e As EventArgs) Handles DGWAnswered.DoubleClick
        EditPhoneEntry(DGWAnswered, Answered)
    End Sub

    Private Sub DGWMissed_DoubleClick(sender As Object, e As EventArgs) Handles DGWMissed.DoubleClick
        EditPhoneEntry(DGWMissed, Missed)
    End Sub

    Private Sub DGWdialled_DoubleClick(sender As Object, e As EventArgs) Handles DGWdialled.DoubleClick
        EditPhoneEntry(DGWdialled, Dialled)
    End Sub


    Private Sub btnQuickDial_Click(sender As Object, e As EventArgs)
        DialFromSpeedDialBox()
    End Sub

    Public Sub CheckForExistingInstance()
        'Get number of processes of you program
        If Process.GetProcessesByName _
          (Process.GetCurrentProcess.ProcessName).Length > 1 Then

            MessageBox.Show _
             ("SPA Call Manager Pro is already running.",
                 "SPA Call Manager Pro",
                  MessageBoxButtons.OK,
                 MessageBoxIcon.Exclamation)
            Application.Exit()
        End If
    End Sub

    Private Function PingHandset() As Boolean
        Return NetUtils.PingHandset(MyStoredPhoneSettings.PhoneIP)
    End Function

    Private Sub TbDirectories_Click(sender As Object, e As EventArgs) Handles TbDirectories.Click

        ' refreshes data from the phone when the tab is clicked so that all data is up to date

        Select Case TbDirectories.SelectedIndex
            Case 0
                'Do nothing (data is bound)
            Case 1
                'Do nothing (data is bound)
            Case 2
                If PingHandset() Then
                    GetPhoneDir("http://" & MyStoredPhoneSettings.PhoneIP & "/pdir.htm")
                    RefillCombinedPhonebook()
                End If
            Case 3
                If PingHandset() Then
                    GetPhoneCalled("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")
                    RefillCombinedPhonebook()
                End If
            Case 4
                If PingHandset() Then
                    GetPhoneAnswered("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")
                    RefillCombinedPhonebook()
                End If
            Case 5
                If PingHandset() Then
                    GetPhoneMissed("http://" & MyStoredPhoneSettings.PhoneIP & "/calllog.htm")
                    RefillCombinedPhonebook()
                End If
        End Select


    End Sub

    Private Sub DGVSharedDir_CellContentClick(sender As Object, e As DataGridViewCellEventArgs) Handles DGVSharedDir.CellContentClick
        If e.RowIndex = -1 Then Return

        If TypeOf (CType(sender, DataGridView).CurrentCell.OwningColumn) Is DataGridViewButtonColumn Then
            Dim result = FindFreeLine() ' finds a free line...ie so if line i is in use it will chosse lone 2 to call out on.
            If result = 0 Then
                MsgBox("No line available")
                Exit Sub
            End If

            LinePhoneStatus(result).Id = result

            Dim entry = MySharedPhoneBook(e.RowIndex)

            LinePhoneStatus(result).CallerNumber = entry.Number
            LinePhoneStatus(result).CallerName = entry.DisplayName
            CmbNumber.Text = LinePhoneStatus(result).CallerName

            Dim callString As String = PhoneAction(EAction.Dial, LinePhoneStatus(result), MyPhoneSettings)
            SendUdp(callString, MyPhoneSettings.PhoneIP, MyStoredPhoneSettings.PhonePort) ' sends data to phone to initiate call
        End If

    End Sub

    Private Sub DGVSharedDir_DoubleClick(sender As Object, e As EventArgs) Handles DGVSharedDir.DoubleClick
        If DGVSharedDir.CurrentCell Is Nothing Then Return

        If Not TypeOf (CType(sender, DataGridView).CurrentCell.OwningColumn) Is DataGridViewButtonColumn Then

            Dim entry = MySharedPhoneBook(DGVSharedDir.CurrentCell.RowIndex)

            Dim newFrmPhonebook As New FrmPhoneBook(entry, DGVSharedDir.CurrentCell.RowIndex, DGVSharedDir.Name)
            newFrmPhonebook.ShowDialog()
        End If

    End Sub

    Private Sub DGVSharedDir_KeyDown(sender As Object, e As KeyEventArgs) Handles DGVSharedDir.KeyDown
        Dim grid = DGVSharedDir

        If e.KeyData = Keys.Delete Then
            Dim entry = MySharedPhoneBook(grid.CurrentCell.RowIndex)

            Dim result As MsgBoxResult = MsgBox("Do you wish to delete" & vbCrLf & entry.DisplayName & "?", MsgBoxStyle.YesNo Or MsgBoxStyle.Critical, "Phone Book")
            If result = MsgBoxResult.Yes Then
                'removes entry from the myphonebook array
                MySharedPhoneBook.Remove(entry)

                SaveSharedPhoneBook(Path.Combine(MyStoredPhoneSettings.sharedDataDir, "Phonebook.csv"))
                LoadSharedPhoneBook(Path.Combine(MyStoredPhoneSettings.sharedDataDir, "Phonebook.csv"))
            End If
        End If

    End Sub

    Private Sub FSW_Changed(sender As Object, e As FileSystemEventArgs) Handles FSW.Changed

        LoadSharedPhoneBook(Path.Combine(MyStoredPhoneSettings.sharedDataDir, "Phonebook.csv"))

    End Sub

End Class