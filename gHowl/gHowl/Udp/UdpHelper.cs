using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace gHowl.Udp
{
    static class UdpHelper
    {


        public static bool IsMulticast(IPAddress ip)
        {
            byte[] ipBytes = ip.GetAddressBytes();
            return (ipBytes.Length == 6 && ip.IsIPv6Multicast) 
                || (ipBytes.Length == 4 && ipBytes[0] >= 224 && ipBytes[0] < 240); 
        }

    }
}
