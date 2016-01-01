Imports System.ComponentModel
Imports System.Xml
Imports System.Text
Imports System.IO
Imports System.Net
Imports System.Net.Sockets

Public Module ClsPhone
    'enum of possible phone status
    Public Enum EPhoneStatus
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
    Public Structure SPhoneStatus
        Dim Status As EPhoneStatus 'enum status
        Dim LineNumber As Integer ' extn line 
        Dim CallerNumber As String  'caller or called number
        Dim CallerName As String 'caller or called name
        Dim Id As Integer
        Dim Ref As Integer
    End Structure

    Public Event UdpRxdata(phoneStatusdata As SPhoneStatus) 'event raised when phone sends data to pc
    Private ReadOnly RemoteIpEndPoint As New IPEndPoint(Net.IPAddress.Any, 0) 'receiving ip address
    Private _strReturnData As String 'data received from phone
    Private ReadOnly ReceivingUdpClient As New UdpClient 'UDP socket
    Private ReadOnly UdpClient As New UdpClient

    'Private ReadOnly Property _Ipaddress As New List(Of String)
    'Public ReadOnly Property Ipaddress As List(Of String)
    '    Get
    '        Return _Ipaddress
    '    End Get
    'End Property

    Dim _phoneSettings As Settings 'current phone settings
    Dim _udpRxPort As Integer = 0
    Dim _userName As String
    Dim _password As String
    Private WithEvents BgwUDP As New BackgroundWorker 'background thread to receive UDP

    Public Property IpPort() As Integer
        'sets the ip port that the app will listen for phone data
        Get
            Return _udpRxPort
        End Get
        Set
            _udpRxPort = Value
        End Set
    End Property

    Public Property Username() As String
        'sets the ip port that the app will listen for phone data
        Get
            Return _userName
        End Get
        Set(value As String)
            _userName = value
        End Set

    End Property

    Public Property Password() As String
        'sets the ip port that the app will listen for phone data
        Get
            Return _password
        End Get
        Set
            _password = Value
        End Set
    End Property

    Public Function DownloadPhoneSettings(ipAddress As String) As Settings
        ' A Function to download settings from the Phones XML configuration file.
        Dim strUrl As String = "http://" & ipAddress & "/admin/spacfg.xml"
        Dim reader As XmlTextReader = New XmlTextReader(strUrl)

        If _password <> "" Then
            Dim cred As New NetworkCredential("admin", _password)
            Dim resolver As XmlUrlResolver = New XmlUrlResolver()
            resolver.Credentials = cred
            reader.XmlResolver = resolver
        End If

        Try
            With _phoneSettings
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
                                    .PhonePort = CInt(reader.Value)
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
            _phoneSettings.PhoneModel = "invalid"
            _phoneSettings.PhoneSoftwareVersion = "invalid"
            _phoneSettings.CTI_Enable = "invalid"
            _phoneSettings.Debug_Server_Address = "invalid"
            _phoneSettings.DebugLevel = "invalid"
            _phoneSettings.StationName = "invalid"
            _phoneSettings.LinksysKeySystem = "invalid"
            ' PhoneSettings.PhonePort = "invalid"
            _phoneSettings.PhonePort = 0
        End Try

        _phoneSettings.LocalIP = MyStoredPhoneSettings.LocalIP

        Return _phoneSettings

    End Function

    Public Function GetLocalIp() As String()

        'function to retrieve local Ip addresses
        Try
            Dim h = Dns.GetHostEntry(Dns.GetHostName)
            Dim result = h.AddressList.Where(
                                        Function(x) x.AddressFamily = AddressFamily.InterNetwork) _
                                      .Select(
                                        Function(x) x.ToString()
                                      )
            Return result.ToArray()

        Catch ex As Exception

            ex.Log()

            Return New String() {"127.0.0.1"}

        End Try

    End Function

#Region "UDP"

    Public Sub Startlistening()
        'starts the listening process for messages from the phone
        Try
            'receivingUdpClient = New System.Net.Sockets.UdpClient(UdpRXPort)
            BgwUDP.RunWorkerAsync()
        Catch ex As Exception
            ex.Log()
        End Try

    End Sub

    Private Sub BgwUDP_DoWork(sender As Object, e As System.ComponentModel.DoWorkEventArgs) Handles BgwUDP.DoWork

        ' background thread to recive UDP messages

        Try
            Dim receiveBytes As [Byte]() = ReceivingUdpClient.Receive(RemoteIpEndPoint)
            _strReturnData = System.Text.Encoding.ASCII.GetString(receiveBytes)
        Catch ex As Exception
            ' MsgBox(ex.Message)
            ex.Log()
        End Try

    End Sub

    Private Sub BgwUDP_RunWorkerCompleted(sender As Object, e As System.ComponentModel.RunWorkerCompletedEventArgs) Handles BgwUDP.RunWorkerCompleted

        'when UDP is received, this function parses the data and raises an event to the client

        Try
            If _strReturnData = "" Then Exit Sub
            If _strReturnData.Contains("<spa-status>") Then
                RaiseEvent UdpRxdata(ProcessInboundPhoneMessage(_strReturnData))
            End If
            BgwUDP.RunWorkerAsync()
        Catch ex As Exception
            ex.Log()
        End Try

    End Sub

    Private Function ProcessInboundPhoneMessage(message As String) As SPhoneStatus

        'Function to handle the various different inbound messages from the phone.  
        On Error Resume Next

        Dim phoneStatus As SPhoneStatus = Nothing
        Dim messageContent As String = ""
        Dim splLn() As String


        messageContent = message.Substring(message.IndexOf("<spa-status>"))

        Dim spl() As String = messageContent.Split(" ".ToCharArray())
        If spl IsNot Nothing Then
            For x As Integer = 0 To spl.GetUpperBound(0)
                splLn = Split(spl(x), "=")
                Select Case splLn(0).Replace(Chr(34), "")
                    Case "id"
                        phoneStatus.Id = CInt(splLn(1).Replace(Chr(34), ""))
                    Case "ref"
                        phoneStatus.Ref = CInt(splLn(1).Replace(Chr(34), ""))
                    Case "ext"
                        phoneStatus.LineNumber = CInt(splLn(1).Replace(Chr(34), ""))
                    Case "state"
                        Select Case splLn(1).Substring(0, splLn(1).IndexOf(Chr(34), 1)).Replace(Chr(34), "")
                            Case "dialing"
                                phoneStatus.Status = EPhoneStatus.Dialing
                            Case "connected"
                                phoneStatus.Status = EPhoneStatus.Connected
                            Case "idle"
                                phoneStatus.Status = EPhoneStatus.Idle
                            Case "ringing"
                                phoneStatus.Status = EPhoneStatus.Ringing
                            Case "answering"
                                phoneStatus.Status = EPhoneStatus.Answering
                            Case "calling"
                                phoneStatus.Status = EPhoneStatus.Calling
                            Case "holding"
                                phoneStatus.Status = EPhoneStatus.Holding
                            Case "hold"
                                phoneStatus.Status = EPhoneStatus.Holding
                            Case Else
                                phoneStatus.Status = EPhoneStatus.Unknown
                        End Select
                    Case "name"
                        If splLn(1).Replace(Chr(34), "") <> "" Then phoneStatus.CallerNumber = splLn(1).Replace(Chr(34), "")
                    Case "uri"
                        If splLn(1).Replace(Chr(34), "") <> "" Then phoneStatus.CallerNumber = splLn(1).Replace(Chr(34), "").Substring(0, splLn(1).IndexOf("@") - 1)
                End Select
            Next
        End If


        Return phoneStatus

    End Function

    Public Sub SendUdp(message As String, ipaddress As String, port As Integer)

        'sends a message to the phone to make phoen do something!
        Try

            Dim bytCommand = Encoding.ASCII.GetBytes(message)
            UdpClient.Connect(ipaddress, port)
            UdpClient.Send(bytCommand, bytCommand.Length)

        Catch ex As Exception
            Dim wrappedException As New UdpMessageException("Error while sending UDP message, see inner exception for details", message, ex)
            wrappedException.Log()
        End Try

    End Sub

#End Region
    Private Sub WriteLog(strdata As String)
        Dim fullPath As String = DataDir & "\CiscoPhone\Calldata.xml"
        Try
            Using reader As New StreamWriter(fullPath, True)
                reader.Write(strdata & vbCrLf & vbCrLf)
            End Using
        Catch ex As Exception
            ex.Log()
        End Try
    End Sub
End Module