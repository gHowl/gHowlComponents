using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace gHowl.Udp
{
    class ClientIpTuple
    {
        UdpClient _client;
        IPEndPoint _ip;
      

        public ClientIpTuple(UdpClient client, IPEndPoint ip)
        {
            _client = client;
            _ip = ip;
          
        }

        public UdpClient Client
        {
            get
            {
                return _client;
            }

        }
        public IPEndPoint Ip
        {
            get
            {
                return _ip;
            }
        }
       
    }
}
