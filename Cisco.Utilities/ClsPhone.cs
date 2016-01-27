using System;
using System.ComponentModel;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;

namespace Cisco.Utilities
{
#pragma warning disable 1591

    public static class ClsPhone
    {
        //enum of possible phone status
        public enum EPhoneStatus
        {
            Idle = 0,
            Ringing = 1,
            Connected = 2,
            Dialing = 3,
            Answering = 4,
            Calling = 5,
            Holding = 6,
            Hold = 7,
            Unknown = 8
        }
        //structure defining current status of phone
        public struct SPhoneStatus
        {
            //enum status
            public EPhoneStatus Status;
            // extn line 
            public int LineNumber;
            //caller or called number
            public string CallerNumber;
            //caller or called name
            public string CallerName;
            public int Id;
            public int Ref;

            public override string ToString()
            {
                return string.Format("[{0}]({1}) {2} <{3}> ({4}:{5})", Status, LineNumber, CallerNumber, CallerName, Id,Ref);
            }
        }

        public static event UdpRxdataEventHandler UdpRxdata;
        public delegate void UdpRxdataEventHandler(SPhoneStatus phoneStatusdata);
        //event raised when phone sends data to pc
        //receiving ip address
        private static IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
        //data received from phone
        private static string _strReturnData;
        //UDP socket
        private static UdpClient ReceivingUdpClient;

        //Private ReadOnly Property _Ipaddress As New List(Of String)
        //Public ReadOnly Property Ipaddress As List(Of String)
        //    Get
        //        Return _Ipaddress
        //    End Get
        //End Property

        //current phone settings
        static Settings _phoneSettings;
        private static BackgroundWorker withEventsField_BgwUDP = new BackgroundWorker();
        private static BackgroundWorker BgwUDP
        {
            get { return withEventsField_BgwUDP; }
            set
            {
                if (withEventsField_BgwUDP != null)
                {
                    withEventsField_BgwUDP.DoWork -= BgwUDP_DoWork;
                    withEventsField_BgwUDP.RunWorkerCompleted -= BgwUDP_RunWorkerCompleted;
                }
                withEventsField_BgwUDP = value;
                if (withEventsField_BgwUDP != null)
                {
                    withEventsField_BgwUDP.DoWork += BgwUDP_DoWork;
                    withEventsField_BgwUDP.RunWorkerCompleted += BgwUDP_RunWorkerCompleted;
                }
            }
            //background thread to receive UDP
        }

        public static int IpPort { get; set; }

        public static string Username { get; set; }

        public static string Password { get; set; }

        /// <summary>
        /// Downloads the phone settings.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="localIp">The local ip address.</param>
        /// <param name="warning">A critical warning.</param>
        /// <returns></returns>
        public static Settings DownloadPhoneSettings(string ipAddress, string localIp, out string warning)
        {
            // A Function to download settings from the Phones XML configuration file.
            string strUrl = "http://" + ipAddress + "/admin/spacfg.xml";

            using (var reader = new XmlTextReader(strUrl))
            {

                if (!string.IsNullOrEmpty(Password))
                {
                    NetworkCredential cred = new NetworkCredential("admin", Password);
                    var resolver = new XmlUrlResolver();
                    resolver.Credentials = cred;
                    reader.XmlResolver = resolver;
                }

                try
                {
                    while ((reader.Read()))
                    {
                        switch (reader.NodeType)
                        {
                            case XmlNodeType.Element:
                                switch (reader.Name)
                                {
                                    case "Product_Name":
                                        reader.Read();
                                        _phoneSettings.PhoneModel = reader.Value;
                                        break;
                                    case "Software_Version":
                                        reader.Read();
                                        _phoneSettings.PhoneSoftwareVersion = reader.Value;
                                        break;
                                    case "CTI_Enable":
                                        reader.Read();
                                        _phoneSettings.CTI_Enable = reader.Value;
                                        break;
                                    case "Debug_Server":
                                        reader.Read();
                                        _phoneSettings.Debug_Server_Address = reader.Value;
                                        break;
                                    case "SIP_Debug_Option_1_":
                                        reader.Read();
                                        _phoneSettings.DebugLevel = reader.Value;
                                        break;
                                    case "Station_Name":
                                        reader.Read();
                                        _phoneSettings.StationName = reader.Value;
                                        break;
                                    case "Linksys_Key_System":
                                        reader.Read();
                                        _phoneSettings.LinksysKeySystem = reader.Value;
                                        break;
                                    case "SIP_Port_1_":
                                        reader.Read();
                                        _phoneSettings.PhonePort = Convert.ToInt32(reader.Value);
                                        break;
                                }
                                break;
                        }
                    }


                    // Catch ex As System.Net.WebException
                    //  MsgBox("Loggin onto this phone requires an admin password.", MsgBoxStyle.Critical, "SPA Call Manager Pro")
                    //  Dim inputfrm As New FrmInput
                    //  inputfrm.ShowDialog()

                    warning = null;
                }
                catch (Exception ex)
                {
                    warning = "Unable to read configuration from " + strUrl + Environment.NewLine + "Error: " + ex.Message;

                    _phoneSettings.PhoneModel = "invalid";
                    _phoneSettings.PhoneSoftwareVersion = "invalid";
                    _phoneSettings.CTI_Enable = "invalid";
                    _phoneSettings.Debug_Server_Address = "invalid";
                    _phoneSettings.DebugLevel = "invalid";
                    _phoneSettings.StationName = "invalid";
                    _phoneSettings.LinksysKeySystem = "invalid";
                    // PhoneSettings.PhonePort = "invalid"
                    _phoneSettings.PhonePort = 0;
                }

            }

            _phoneSettings.LocalIP = localIp;

            return _phoneSettings;

        }

        public static string[] GetLocalIp()
        {

            //function to retrieve local Ip addresses
            try
            {
                return NetUtils.GetLocalIpv4Addresses();


            }
            catch (Exception ex)
            {
                ex.Log();

                return new string[] { "127.0.0.1" };

            }

        }

        #region "UDP"

        public static void Startlistening()
        {
            //starts the listening process for messages from the phone
            try
            {
                ReceivingUdpClient = new System.Net.Sockets.UdpClient(IpPort);
                BgwUDP.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                ex.Log();
            }

        }


        private static void BgwUDP_DoWork(object sender, DoWorkEventArgs e)
        {
            // background thread to recive UDP messages

            try
            {
                var receiveBytes = ReceivingUdpClient.Receive(ref RemoteIpEndPoint);
                _strReturnData = Encoding.ASCII.GetString(receiveBytes);
            }
            catch (Exception ex)
            {
                // MsgBox(ex.Message)
                ex.Log();
            }

        }


        private static void BgwUDP_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //when UDP is received, this function parses the data and raises an event to the client

            try
            {
                if (string.IsNullOrEmpty(_strReturnData))
                    return;
                if (_strReturnData.Contains("<spa-status>"))
                {
                    if (UdpRxdata != null)
                    {
                        UdpRxdata(ProcessInboundPhoneMessage(_strReturnData));
                    }
                }
                BgwUDP.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                ex.Log();
            }

        }

        internal static SPhoneStatus ProcessInboundPhoneMessage(string message)
        {

            //Function to handle the various different inbound messages from the phone.  

            SPhoneStatus phoneStatus = default(SPhoneStatus);
            
            try
            {
                var messageContent = message.Substring(message.IndexOf("<spa-status>", StringComparison.InvariantCultureIgnoreCase));

                var spl = messageContent.Split(" ".ToCharArray());
                if (spl != null)
                {

                    for (var x = 0; x <= spl.Length; x++)
                    {
                        if (!spl[x].Contains("=")) continue;

                        var splLn = spl[x].Split(new[] { '=', '/' });

                        try
                        {
                            string value = splLn[1].Trim(" \"".ToCharArray());
                            string name = splLn[0];
                            switch (name.Replace("\"", ""))
                            {
                                case "id":
                                    phoneStatus.Id = Convert.ToInt32(value);
                                    break;
                                case "ref":
                                    phoneStatus.Ref = Convert.ToInt32(value);
                                    break;
                                case "ext":
                                    phoneStatus.LineNumber = Convert.ToInt32(value);
                                    break;
                                case "state":

                                    switch (value)
                                    {
                                        case "dialing":
                                            phoneStatus.Status = EPhoneStatus.Dialing;
                                            break;
                                        case "connected":
                                            phoneStatus.Status = EPhoneStatus.Connected;
                                            break;
                                        case "idle":
                                            phoneStatus.Status = EPhoneStatus.Idle;
                                            break;
                                        case "ringing":
                                            phoneStatus.Status = EPhoneStatus.Ringing;
                                            break;
                                        case "answering":
                                            phoneStatus.Status = EPhoneStatus.Answering;
                                            break;
                                        case "calling":
                                            phoneStatus.Status = EPhoneStatus.Calling;
                                            break;
                                        case "holding":
                                            phoneStatus.Status = EPhoneStatus.Holding;
                                            break;
                                        case "hold":
                                            phoneStatus.Status = EPhoneStatus.Holding;
                                            break;
                                        default:
                                            phoneStatus.Status = EPhoneStatus.Unknown;
                                            break;
                                    }
                                    break;
                                case "name":
                                    if (!string.IsNullOrWhiteSpace(value))
                                        phoneStatus.CallerNumber = value;
                                    break;
                                case "uri":
                                    if (!string.IsNullOrWhiteSpace(value))
                                        phoneStatus.CallerNumber = value.Substring(0, value.IndexOf("@") - 1);
                                    break;
                            }
                        }
                        catch (Exception ex)
                        {
                            (new Exception(string.Format("Exception while parsing '{0}'", splLn), ex)).Log();
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                ex.Log();
            }

            return phoneStatus;
        }


        public static void SendUdp(string message, string ipaddress, int port)
        {
            //sends a message to the phone to make phoen do something!

            try
            {
                var bytCommand = Encoding.ASCII.GetBytes(message);
                var client = new UdpClient();
                client.Connect(ipaddress, port);
                client.Send(bytCommand, bytCommand.Length);
            }
            catch (Exception ex)
            {
                UdpMessageException wrappedException = new UdpMessageException(string.Format("Error while sending UDP message, see inner exception for details. UDP Message was : {0}", message), message, ex);
                wrappedException.Log();
            }

        }

        #endregion

    }
}