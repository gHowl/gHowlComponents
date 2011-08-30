using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using Bespoke.Common.Net;

namespace Bespoke.Common.Osc
{
    /// <summary>
    /// 
    /// </summary>
    public class OscClient
    {
        /// <summary>
        /// 
        /// </summary>
        public IPAddress ServerIPAddress
        {
            get
            {
                return mServerIPAddress;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ServerPort
        {
            get
            {
                return mServerPort;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public TcpClient Client
        {
            get
            {
                return mClient;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public OscClient()
        {
            mClient = new TcpClient();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverIPAddress"></param>
        /// <param name="serverPort"></param>
        public OscClient(IPAddress serverIPAddress, int serverPort)
            : this()
        {
            mServerIPAddress = serverIPAddress;
            mServerPort = serverPort;
        }

        /// <summary>
        /// 
        /// </summary>
        public void Connect()
        {
            Connect(mServerIPAddress, mServerPort);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="serverIPAddress"></param>
        /// <param name="serverPort"></param>
        public void Connect(IPAddress serverIPAddress, int serverPort)
        {
            mClient.Connect(serverIPAddress, serverPort);
            //mTcpConnection = new TcpConnection(mClient.Client, null);
        }

        /// <summary>
        /// 
        /// </summary>
        public void Close()
        {
            //mTcpConnection.Dispose();
            //mTcpConnection = null;
            mClient.Close();            
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="packet"></param>
        public void Send(OscPacket packet)
        {
            byte[] packetData = packet.ToByteArray();
            //mTcpConnection.Writer.Write(OscPacket.ValueToByteArray(packetData));
        }

        private IPAddress mServerIPAddress;
        private int mServerPort;
        private TcpClient mClient;
        //private TcpConnection mTcpConnection; //FIX
    }
}
