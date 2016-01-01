using System.Linq;
using System.Net;
using System.Net.Sockets;

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
    }
}
