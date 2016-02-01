Imports System.ComponentModel
Imports System.Xml
Imports System.Text
Imports System.Net
Imports System.Net.Sockets
Imports Cisco.Utilities

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

        Public Overrides Function ToString() As String
            Return String.Format("[{0}]({1}) {2} <{3}> ({4}:{5})", Status, LineNumber, CallerNumber, CallerName, Id, Ref)
        End Function
    End Structure

    Public Event UdpRxdata(phoneStatusdata As SPhoneStatus) 'event raised when phone sends data to pc
    Private ReadOnly RemoteIpEndPoint As New IPEndPoint(IPAddress.Any, 0) 'receiving ip address
    Private _strReturnData As String 'data received from phone
    Private ReceivingUdpClient As UdpClient 'UDP socket

    'Private ReadOnly Property _Ipaddress As New List(Of String)
    'Public ReadOnly Property Ipaddress As List(Of String)
    '    Get
    '        Return _Ipaddress
    '    End Get
    'End Property

    Dim _phoneSettings As Settings 'current phone settings
    Private WithEvents BgwUDP As New BackgroundWorker 'background thread to receive UDP

    Public Property IpPort As Integer = 0

    Public Property Username As String

    Public Property Password As String

    Public Function DownloadPhoneSettings(ipAddress As String) As Settings
        ' A Function to download settings from the Phones XML configuration file.
        Dim strUrl As String = "http://" & ipAddress & "/admin/spacfg.xml"

        Using reader = New XmlTextReader(strUrl)

            If Password <> "" Then
                Dim cred As New NetworkCredential("admin", Password)
                Dim resolver = New XmlUrlResolver()
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

        End Using

        _phoneSettings.LocalIP = MyStoredPhoneSettings.LocalIP

        Return _phoneSettings

    End Function

    Public Function GetLocalIp() As String()

        'function to retrieve local Ip addresses
        Try
            Return NetUtils.GetLocalIpv4Addresses()

        Catch ex As Exception

            ex.Log()

            Return New String() {"127.0.0.1"}

        End Try

    End Function

#Region "UDP"

    Public Sub Startlistening()
        'starts the listening process for messages from the phone
        Try
            ReceivingUdpClient = New System.Net.Sockets.UdpClient(IpPort)
            BgwUDP.RunWorkerAsync()
        Catch ex As Exception
            ex.Log()
        End Try

    End Sub

    Private Sub BgwUDP_DoWork(sender As Object, e As DoWorkEventArgs) Handles BgwUDP.DoWork

        ' background thread to recive UDP messages

        Try
            Dim receiveBytes = ReceivingUdpClient.Receive(RemoteIpEndPoint)
            _strReturnData = Encoding.ASCII.GetString(receiveBytes)
        Catch ex As Exception
            ' MsgBox(ex.Message)
            ex.Log()
        End Try

    End Sub

    Private Sub BgwUDP_RunWorkerCompleted(sender As Object, e As RunWorkerCompletedEventArgs) Handles BgwUDP.RunWorkerCompleted

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

    Friend Function ProcessInboundPhoneMessage(message As String) As SPhoneStatus

        'Function to handle the various different inbound messages from the phone.  

        Dim phoneStatus As SPhoneStatus = Nothing

        Try

            Dim messageContent = message.Substring(message.IndexOf("<spa-status>", StringComparison.InvariantCultureIgnoreCase))

            Dim spl() As String = messageContent.Split(" ".ToCharArray())
            If spl IsNot Nothing Then
                For Each part In spl

                    If (Not part.Contains("=")) Then Continue For

                    Dim splLn = part.Split("=/".ToCharArray())

                    Try
                        Dim value As String = splLn(1).Trim(" """.ToCharArray())
                        Dim name As String = splLn(0)
                        Select Case name.Replace(Chr(34), "")
                            Case "id"
                                phoneStatus.Id = CInt(value)
                            Case "ref"
                                phoneStatus.Ref = CInt(value)
                            Case "ext"
                                phoneStatus.LineNumber = CInt(value)
                            Case "state"

                                Select Case value
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
                                If Not String.IsNullOrWhiteSpace(value) Then phoneStatus.CallerNumber = value
                            Case "uri"
                                If Not String.IsNullOrWhiteSpace(value) Then phoneStatus.CallerNumber = value.Substring(0, value.IndexOf("@") - 1)
                        End Select
                    Catch ex As Exception
                        With (New Exception(String.Format("Exception while parsing '{0}' from message {1} : {2}", splLn, message, ex), ex))
                            .Log()
                        End With
                        'Log and resume next
                    End Try
                Next
            End If

        Catch ex As Exception
            With (New Exception(String.Format("Exception while parsing '{0}'", message), ex))
                .Log()
            End With
        End Try

        Return phoneStatus
    End Function

    Public Sub SendUdp(message As String, ipaddress As String, port As Integer)

        'sends a message to the phone to make phoen do something!
        Try

            Dim bytCommand = Encoding.ASCII.GetBytes(message)
            With New UdpClient
                .Connect(ipaddress, port)
                .Send(bytCommand, bytCommand.Length)
            End With


        Catch ex As Exception
            Dim wrappedException As New UdpMessageException(String.Format("Error while sending UDP message, see inner exception for details. UDP Message was : {0}", message), message, ex)
            wrappedException.Log()
        End Try

    End Sub

#End Region

End Module