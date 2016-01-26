namespace Cisco.Utilities
{
    /// <summary>
    /// A stataic placeholder for <see cref="Settings"/>
    /// </summary>
    public static class MySettings {
        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public static Settings MyStoredPhoneSettings { get; set; }
    }

    /// <summary>
    /// Phone settings
    /// </summary>
    public struct Settings
    {
        ///<remarks>Refers to <c>Product_Name</c></remarks>
        public string PhoneModel;
        ///<remarks>Refers to Software_Version</remarks>
        public string PhoneSoftwareVersion;
        ///<remarks>Refers to CTI_Enable</remarks>
        public string CTI_Enable;
        ///<summary>Address the phone will send data to</summary>
        public string Debug_Server_Address;
        ///<remarks>Refers to Station_Name</remarks>
        public string StationName;
        ///<remarks>Refers to SIP_Debug_Option_1_</remarks>
        public string DebugLevel;
        
        /// <summary>local Ip address of the PC</summary>
        public string LocalIP;
        /// <summary>Pc port</summary>
        public int LocalPort;
        /// <summary>Ipaddress of phone</summary>
        public string PhoneIP;
        /// <summary>phone port</summary>
        public int PhonePort;
        /// <summary>Phone username</summary>
        public string username;
        /// <summary>Phone password</summary>
        public string password;
        /// <summary>Linksys Key System Setting</summary>
        public string LinksysKeySystem;
        /// <summary>Shared data directory (excluding filename)</summary>
        public string sharedDataDir;
    }
}