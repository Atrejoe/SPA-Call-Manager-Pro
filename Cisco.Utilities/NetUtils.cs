using System.Linq;
using System.Net;
using System.Net.Sockets;
using System;
using System.Data;
using System.Net.NetworkInformation;

namespace Cisco.Utilities
{
    /// <summary>
    /// Network utils
    /// </summary>
    public static class NetUtils
    {
        /// <summary>
        /// Gets the local ipv4 addresses.
        /// </summary>
        /// <returns></returns>
        public static string[] GetLocalIpv4Addresses()
        {
            var h = Dns.GetHostEntry(Dns.GetHostName());

            var result = h.AddressList.Where(x => x.AddressFamily == AddressFamily.InterNetwork)
                .Select(
                    x => x.ToString()
                );
            return result.ToArray();
        }

        /// <summary>
        /// Pings the handset, as setup in <see cref="Settings.PhoneIP" />
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <returns>
        ///   <c>true</c> when ping was successfull, otherwise <c>false</c>.
        /// </returns>
        static public bool PingHandset(string ipAddress) {
            Exception ignoredException = null; 
            return PingHandset(ipAddress, ref ignoredException);
        }

        /// <summary>
        /// Pings the handset, as setup in <see cref="Settings.PhoneIP" />
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="exception">Optional exception, will be specified when pinging not only was unsuccessfull, be threw an exception too.</param>
        /// <returns>
        ///   <c>true</c> when ping was successfull, otherwise <c>false</c>.
        /// </returns>
        static public bool PingHandset(string ipAddress, ref Exception exception)
        {
            var result = false;
            try
            {
                return (new Ping()).Send(ipAddress).Status == IPStatus.Success;
            }
            catch (PingException ex)
            {
                exception = ex;
            }
            return result;
        }

}
}
