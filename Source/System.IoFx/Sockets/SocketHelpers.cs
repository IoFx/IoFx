using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace System.IoFx.Sockets
{
    public class SocketHelpers
    {
        public static IPEndPoint GetFirstIpEndPointFromHostName(string hostName, int port)
        {
            var addresses = System.Net.Dns.GetHostAddresses(hostName);
            if (addresses.Length == 0)
            {
                throw new ArgumentException(
                    "Unable to retrieve address from specified host name.",
                    "hostName"
                );
            }

            return new IPEndPoint(addresses[0], port); // Port gets validated here.
        }
    }
}
