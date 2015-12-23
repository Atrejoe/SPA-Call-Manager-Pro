Imports System.Xml
Imports System.Text
Imports System.IO
Imports System.Net

Public Class ClsPhone
    'enum of possible phone status
    Public Enum ePhoneStatus
        Idle = 0
        Ringing = 1
        Connected = 2
        Dialing = 3
        Answering = 4
        Calling = 5
        Holding = 6
        Hold = 7
        Unknown = 8
    End Enum
    'structure defining current status of phone
    Public Structure sPhoneStatus
        Dim Status As ePhoneStatus 'enum status
        Dim LineNumber As Integer ' extn line 
        Dim CallerNumber As String  'caller or called number
        Dim CallerName As String 'caller or called name
        Dim Id As Integer
        Dim ref As Integer
    End Structure

    Public Event UDPRxdata(ByVal PhoneStatusdata As sPhoneStatus) 'event raised when phone sends data to pc
    Public RemoteIpEndPoint As New System.Net.IPEndPoint(System.Net.IPAddress.Any, 0) 'receiving ip address
    Public strReturnData As String 'data received from phone
    Public receivingUdpClient As New Net.Sockets.UdpClient 'UDP socket
    Public udpClient As New Net.Sockets.UdpClient
    Public Ipaddress() As String
    Dim PhoneSettings As Settings 'durrent phone settings
    Dim UdpRXPort As Integer = 0
    Dim _UserName As String
    Dim _password As String
    Dim WithEvents BgwUDP As New System.ComponentModel.BackgroundWorker 'background thread to recive UDP

    Public Property IpPort() As Integer
        'sets the ip port that the app will listen for phone data
        Get
            Return UdpRXPort
        End Get
        Set(ByVal value As Integer)
            UdpRXPort = value
        End Set

    End Property

    Public Property username() As String
        'sets the ip port that the app will listen for phone data
        Get
            Return _UserName
        End Get
        Set(ByVal value As String)
            _UserName = value
        End Set

    End Property

    Public Property password() As String
        'sets the ip port that the app will listen for phone data
        Get
            Return _password
        End Get
        Set(ByVal value As String)
            _password = value
        End Set

    End Property
    Public Function DownloadPhoneSettings(ByVal IPAddress As String) As Settings
        ' A Function to download settings from the Phones XML configuration file.
        Dim strUrl As String = "http://" & IPAddress & "/admin/spacfg.xml"
        Dim reader As XmlTextReader = New XmlTextReader(strUrl)

        If _password <> "" Then
            Dim cred As New NetworkCredential("admin", _password)
            Dim resolver As XmlUrlResolver = New XmlUrlResolver()
            resolver.Credentials = cred
            reader.XmlResolver = resolver
        End If

        Try
            With PhoneSettings
                Do While (reader.Read())
                    Select Case reader.NodeType
                        Case XmlNodeType.Element
                            Select Case reader.Name
                                Case "Product_Name"
                                    reader.Read()
                                    .PhoneModel = reader.Value
                                Case "Software_Version"
                                    reader.Read()
                                    .PhoneSoftwareVersion = reader.Value
                                Case "CTI_Enable"
                                    reader.Read()
                                    .CTI_Enable = reader.Value
                                Case "Debug_Server"
                                    reader.Read()
                                    .Debug_Server_Address = reader.Value
                                Case "SIP_Debug_Option_1_"
                                    reader.Read()
                                    .DebugLevel = reader.Value
                                Case "Station_Name"
                                    reader.Read()
                                    .StationName = reader.Value
                                Case "Linksys_Key_System"
                                    reader.Read()
                                    .LinksysKeySystem = reader.Value
                                Case "SIP_Port_1_"
                                    reader.Read()
                                    .PhonePort = Cint(reader.Value)
                            End Select
                    End Select
                Loop
            End With
            ' Catch ex As System.Net.WebException
            '  MsgBox("Loggin onto this phone requires an admin password.", MsgBoxStyle.Critical, "SPA Call Manager Pro")
            '  Dim inputfrm As New FrmInput
            '  inputfrm.ShowDialog()
        Catch ex As Exception
            MsgBox("Unable to read configuration from " & strUrl & vbCrLf & "Error: " & ex.Message, MsgBoxStyle.Critical, "SPA Call Manager Pro")
            PhoneSettings.PhoneModel = "invalid"
            PhoneSettings.PhoneSoftwareVersion = "invalid"
            PhoneSettings.CTI_Enable = "invalid"
            PhoneSettings.Debug_Server_Address = "invalid"
            PhoneSettings.DebugLevel = "invalid"
            PhoneSettings.StationName = "invalid"
            PhoneSettings.LinksysKeySystem = "invalid"
            ' PhoneSettings.PhonePort = "invalid"
            PhoneSettings.PhonePort = 0
        End Try

        PhoneSettings.LocalIP = MyStoredPhoneSettings.LocalIP

        Return PhoneSettings

    End Function

    Public Function GetLocalIp() As String()

        'function top retrieve local Ip address
        Dim Ipcounter As Integer = 0
        Try
            Dim h As System.Net.IPHostEntry = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName)
            ReDim Ipaddress(h.AddressList.GetUpperBound(0))
            For x As Integer = 0 To h.AddressList.GetUpperBound(0)
                If h.AddressList(x).AddressFamily <> Net.Sockets.AddressFamily.InterNetworkV6 Then
                    ReDim Preserve Ipaddress(Ipcounter)
                    Ipaddress(Ipcounter) = h.AddressList.GetValue(x).ToString
                    Ipcounter += 1
                End If
            Next
            Return Ipaddress
        Catch ex As Exception
            ReDim Ipaddress(0)
            Ipaddress(0) = "127.0.0.1"
            Return Ipaddress
        End Try


    End Function

    Public Sub New()

        'inistialises class and passes the UDP socket port to listen on

    End Sub

#Region "UDP"

    Public Sub Startlistening()
        'starts the listening process for messages from the phine
        Try
            receivingUdpClient = New System.Net.Sockets.UdpClient(UdpRXPort)
            BgwUDP.RunWorkerAsync()
        Catch ex As Exception

        End Try

    End Sub

    Private Sub BgwUDP_DoWork(ByVal sender As Object, ByVal e As System.ComponentModel.DoWorkEventArgs) Handles BgwUDP.DoWork

        ' background thread to recive UDP messages

        Try
            Dim receiveBytes As [Byte]() = receivingUdpClient.Receive(RemoteIpEndPoint)
            strReturnData = System.Text.Encoding.ASCII.GetString(receiveBytes)
        Catch ex As Exception
            ' MsgBox(ex.Message)
        End Try

    End Sub

    Private Sub BgwUDP_RunWorkerCompleted(ByVal sender As Object, ByVal e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BgwUDP.RunWorkerCompleted

        'when UDP is received, this function parses the data and raises an event to the client

        Try
            If strReturnData = "" Then Exit Sub
            If strReturnData.Contains("<spa-status>") Then
                RaiseEvent UDPRxdata(ProcessInboundPhoneMessage(strReturnData))
            End If
            BgwUDP.RunWorkerAsync()
        Catch ex As Exception

        End Try

    End Sub

    Private Function ProcessInboundPhoneMessage(ByVal message As String) As sPhoneStatus

        'Function to handle the various different inbound messages from the phone.  
        On Error Resume Next

        Dim PhoneStatus As sPhoneStatus = Nothing
        Dim MessageContent As String = ""
        Dim SplLn() As String


        MessageContent = message.Substring(message.IndexOf("<spa-status>"))

        Dim spl() As String = MessageContent.Split(" ".ToCharArray())
        If spl IsNot Nothing Then
            For x As Integer = 0 To spl.GetUpperBound(0)
                SplLn = Split(spl(x), "=")
                Select Case SplLn(0).Replace(Chr(34), "")
                    Case "id"
                        PhoneStatus.Id = CInt(SplLn(1).Replace(Chr(34), ""))
                    Case "ref"
                        PhoneStatus.ref = CInt(SplLn(1).Replace(Chr(34), ""))
                    Case "ext"
                        PhoneStatus.LineNumber = CInt(SplLn(1).Replace(Chr(34), ""))
                    Case "state"
                        Select Case SplLn(1).Substring(0, SplLn(1).IndexOf(Chr(34), 1)).Replace(Chr(34), "")
                            Case "dialing"
                                PhoneStatus.Status = ePhoneStatus.Dialing
                            Case "connected"
                                PhoneStatus.Status = ePhoneStatus.Connected
                            Case "idle"
                                PhoneStatus.Status = ePhoneStatus.Idle
                            Case "ringing"
                                PhoneStatus.Status = ePhoneStatus.Ringing
                            Case "answering"
                                PhoneStatus.Status = ePhoneStatus.Answering
                            Case "calling"
                                PhoneStatus.Status = ePhoneStatus.Calling
                            Case "holding"
                                PhoneStatus.Status = ePhoneStatus.Holding
                            Case "hold"
                                PhoneStatus.Status = ePhoneStatus.Holding
                            Case Else
                                PhoneStatus.Status = ePhoneStatus.Unknown
                        End Select
                    Case "name"
                        If SplLn(1).Replace(Chr(34), "") <> "" Then PhoneStatus.CallerNumber = SplLn(1).Replace(Chr(34), "")
                    Case "uri"
                        If SplLn(1).Replace(Chr(34), "") <> "" Then PhoneStatus.CallerNumber = SplLn(1).Replace(Chr(34), "").Substring(0, SplLn(1).IndexOf("@") - 1)
                End Select
            Next
        End If


        Return PhoneStatus

    End Function

    Public Sub SendUdp(ByVal Message As String, ByVal Ipaddress As String, ByVal Port As Integer)

        'sends a message to the phone to make phoen do something!
        Try
            Dim bytCommand As Byte() = New Byte() {}

            udpClient.Connect(Ipaddress, Port)
            bytCommand = Encoding.ASCII.GetBytes(Message)
            Dim pRet As Integer = udpClient.Send(bytCommand, bytCommand.Length)

        Catch ex As Exception

        End Try

    End Sub

#End Region
    Private Sub WriteLog(strdata As String)
        'Dim bAns As Boolean = False
        Dim objReader As StreamWriter

        Dim FullPath As String = dataDir & "\CiscoPhone\Calldata.xml"
        Try
            objReader = New StreamWriter(FullPath, True)
            objReader.Write(strdata & vbCrLf & vbCrLf)
            objReader.Close()
        Catch Ex As Exception
        End Try
    End Sub
End Class