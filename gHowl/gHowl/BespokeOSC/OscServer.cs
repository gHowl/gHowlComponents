using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Bespoke.Common.Net;
//using Bespoke.Common.Net;

namespace Bespoke.Common.Osc
{
	/// <summary>
	/// Method signature for raising OscPacketReceived events.
	/// </summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">An EventArgs object that contains the event data.</param>
	public delegate void OscPacketReceivedHandler(object sender, OscPacketReceivedEventArgs e);

	/// <summary>
	/// Method signature for raising OscBundleReceived events.
	/// </summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">An EventArgs object that contains the event data.</param>
	public delegate void OscBundleReceivedHandler(object sender, OscBundleReceivedEventArgs e);

	/// <summary>
	/// Method signature for raising OscMessageReceived events.
	/// </summary>
	/// <param name="sender">The sender of the event.</param>
	/// <param name="e">An EventArgs object that contains the event data.</param>
	public delegate void OscMessageReceivedHandler(object sender, OscMessageReceivedEventArgs e);

    /// <summary>
	/// Listens for and processes incoming Open Sound Control packets.
	/// </summary>
	public class OscServer
	{
		#region Events

		/// <summary>
		/// Raised when an OscPacket is received.
		/// </summary>
		public event OscPacketReceivedHandler PacketReceived;

		/// <summary>
		/// Raised when an OscBundle is received.
		/// </summary>
		public event OscBundleReceivedHandler BundleReceived;

		/// <summary>
		/// Raised when an OscMessage is received.
		/// </summary>
		public event OscMessageReceivedHandler MessageReceived;

		#endregion

        /// <summary>
        /// Gets the selected transport type.
        /// </summary>
        public TransportType TransportType
        {
            get
            {
                return mTransportType;
            }
        }

		/// <summary>
		/// Gets the local IP address the server is bound to.
		/// </summary>
		/// <remarks>Not used for UDP multicast.</remarks>
		public IPAddress IPAddress
		{
			get
			{
				return mIPAddress;
			}
		}

		/// <summary>
		/// Gets the local port number the server is bound to.
		/// </summary>
		public int Port
		{
			get
			{
				return mPort;
			}
		}

        /// <summary>
        /// Gets the local IP endpoint the server is bound to.
        /// </summary>
        public IPEndPoint IPEndPoint
        {
            get
            {
                return mIPEndPoint;
            }
        }

		/// <summary>
		/// (Optional) Gets the multicast IP address the server is a member of.
		/// </summary>
		/// <remarks>Not used for UDP unicast.</remarks>
		public IPAddress MulticastAddress
		{
			get
			{
				return mMulticastAddress;
			}
		}

		/// <summary>
		/// Gets all registered OSC methods (address patterns).
		/// </summary>
		public string[] RegisteredMethods
		{
			get
			{
				return mRegisteredMethods.ToArray();
			}
		}

		/// <summary>
		/// Specifies if incoming OSC messages should be filtered against the registered methods.
		/// </summary>
		public bool FilterRegisteredMethods
		{
			get
			{
				return mFilterRegisteredMethods;
			}
			set
			{
				mFilterRegisteredMethods = value;
			}
		}

		/// <summary>
		/// Gets the transmission type being used by the server.
		/// </summary>
		public TransmissionType TransmissionType
		{
			get
			{
				return mTransmissionType;
			}
		}

        /// <summary>
        /// Gets the status of the server.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                return mHandleMessages;
            }
        }

		/// <summary>
		/// Creates a new instance of OscServer.
		/// </summary>
        /// <param name="transportType">The underlying transport protocol.</param>
		/// <param name="ipAddress">The local IP address to bind to.</param>
		/// <param name="port">The UDP port to bind to.</param>
		/// <remarks>TransmissionType.Unicast.</remarks>
		public OscServer(TransportType transportType, IPAddress ipAddress, int port)
			: this(transportType, ipAddress, port, null, TransmissionType.Unicast)
		{
		}

		/// <summary>
		/// Creates a new instance of OscServer.
		/// </summary>
		/// <param name="port">The UDP port to bind to.</param>
		/// <param name="multicastAddress">The multicast IP address to join.</param>
		/// <remarks>TransmissionType.Multicast.</remarks>
		public OscServer(int port, IPAddress multicastAddress)
			: this(TransportType.Udp, IPAddress.Loopback, port, multicastAddress, TransmissionType.Multicast)
		{
		}

		/// <summary>
		/// Creates a new instance of OscServer.
		/// </summary>
        /// <param name="transportType">The underlying transport protocol.</param>
		/// <param name="ipAddress">The local IP address to bind to.</param>
		/// <param name="port">The UDP port to bind to.</param>
		/// <param name="multicastAddress">The multicast IP address to join.</param>
		/// <param name="transmissionType">The transmission type for the server to use.</param>
		/// <remarks>If ipAddress is specified, Unicast; otherwise, if multicastAddress is specified, Multicast.</remarks>
		public OscServer(TransportType transportType, IPAddress ipAddress, int port, IPAddress multicastAddress, TransmissionType transmissionType)
		{
            Assert.IsTrue(transportType == TransportType.Udp || transportType == TransportType.Tcp); 
            if ((transportType == TransportType.Tcp) && (transmissionType != TransmissionType.Unicast))
            {
                throw new InvalidOperationException("TCP must be used with TransmissionType.Unicast.");
            }

            mTransportType = transportType;
			mIPAddress = ipAddress;
			mPort = port;
            mIPEndPoint = new IPEndPoint(ipAddress, port);
			mTransmissionType = transmissionType;

			if (mTransmissionType == TransmissionType.Multicast)
			{
				Assert.ParamIsNotNull(multicastAddress);
				mMulticastAddress = multicastAddress;
			}

			mRegisteredMethods = new List<string>();
			mFilterRegisteredMethods = true;
            
            switch (mTransportType)
            {
                case TransportType.Udp:
                    mUdpServer = new UdpServer(mIPAddress, mPort, mMulticastAddress, mTransmissionType);
                    mUdpServer.DataReceived += new UdpDataReceivedHandler(mUdpServer_DataReceived);
                    break;

                //case TransportType.Tcp:                   
                //    mTcpServer = new TcpServer(mIPAddress, mPort, true, true);
                //    mTcpServer.DataReceived += new TcpDataReceivedHandler(mTcpServer_DataReceived);
                //    break;

                default:
                    throw new InvalidOperationException("Invalid transport type: " + mTransportType.ToString());
            }
		}

#if WINDOWS
        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsIPEndPointAvailable(IPAddress ipAddress, int port)
        {
            return UdpServer.IsIPEndPointAvailable(ipAddress, port);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipEndPoint"></param>
        /// <returns></returns>
        public static bool IsIPEndPointAvailable(IPEndPoint ipEndPoint)
        {
            return UdpServer.IsIPEndPointAvailable(ipEndPoint);
        }
#endif

		/// <summary>
		/// Start listening for incoming OSC packets.
		/// </summary>
		public void Start()
		{
			mHandleMessages = true;

            switch (mTransportType)
            {
                case TransportType.Udp:
                    mUdpServer.Start();
                    break;

                //case TransportType.Tcp:
                //    mTcpServerThread = new Thread(mTcpServer.Start);
                //    mTcpServerThread.Name = "OscServer Thread";
                //    mTcpServerThread.Start();
                //    break;

                default:
                    throw new InvalidOperationException("Invalid transport type: " + mTransportType.ToString());
            }
		}

		/// <summary>
		/// Stop listening for Osc packets.
		/// </summary>
		public void Stop()
		{
			mHandleMessages = false;

            switch (mTransportType)
            {
                case TransportType.Udp:
                    if (mUdpServer != null)
                    {
                        mUdpServer.Stop();
                    }
                    break;

                //case TransportType.Tcp:
                //    if (mTcpServer != null)
                //    {
                //        mTcpServer.Stop();
                //        if (mTcpServerThread != null)
                //        {
                //            mTcpServerThread.Join();
                //            mTcpServerThread = null;
                //        }
                //    }
                //    break;

                default:
                    throw new InvalidOperationException("Invalid transport type: " + mTransportType.ToString());
            }
		}

		/// <summary>
		/// Register an OSC method.
		/// </summary>
		/// <param name="method">The OSC address pattern to register.</param>
		public void RegisterMethod(string method)
		{
			if (mRegisteredMethods.Contains(method) == false)
			{
				mRegisteredMethods.Add(method);
			}
		}

		/// <summary>
		/// Unregister an OSC method.
		/// </summary>
		/// <param name="method">The OSC address pattern to unregister.</param>
		public void UnRegisterMethod(string method)
		{
			mRegisteredMethods.Remove(method);
		}

		/// <summary>
		/// Unregister all OSC methods.
		/// </summary>
		public void ClearMethods()
		{
			mRegisteredMethods.Clear();
		}

		#region Private Methods

		/// <summary>
		/// Process data received events.
		/// </summary>
		/// <param name="sender">The sender of the event.</param>
		/// <param name="e">An EventArgs object that contains the event data.</param>
		private void mUdpServer_DataReceived(object sender, UdpDataReceivedEventArgs e)
		{
            DataReceived(e.SourceEndPoint, e.Data);
		}

        /// <summary>
        /// Process data received events.
        /// </summary>
        /// <param name="sender">The sender of the event.</param>
        /// <param name="e">An EventArgs object that contains the event data.</param>
        //private void mTcpServer_DataReceived(object sender, TcpDataReceivedEventArgs e)
        //{
        //    DataReceived((IPEndPoint)e.Connection.Client.RemoteEndPoint, e.Data);            
        //}

        /// <summary>
        /// Process data received event.
        /// </summary>
        /// <param name="sourceEndPoint"></param>
        /// <param name="data">The received data.</param>
        private void DataReceived(IPEndPoint sourceEndPoint, byte[] data)
        {
            if (mHandleMessages)
            {
                OscPacket packet = OscPacket.FromByteArray(sourceEndPoint, data);
                OnPacketReceived(packet);

                if (packet.IsBundle)
                {
                    OnBundleReceived(packet as OscBundle);

                    foreach (object value in packet.Data)
                    {
                        OscMessage message = value as OscMessage;
                        if (message != null)
                        {
                            if (mFilterRegisteredMethods)
                            {
                                if (mRegisteredMethods.Contains(message.Address))
                                {
                                    OnMessageReceived(message);
                                }
                            }
                            else
                            {
                                OnMessageReceived(message);
                            }
                        }
                    }
                }
                else
                {
                    if (mFilterRegisteredMethods)
                    {
                        if (mRegisteredMethods.Contains(packet.Address))
                        {
                            OnMessageReceived(packet as OscMessage);
                        }
                    }
                    else
                    {
                        OnMessageReceived(packet as OscMessage);
                    }
                }
            }
        }

		/// <summary>
		/// Raises the PacketreceivedEvent.
		/// </summary>
		/// <param name="packet">The packet to include in the event arguments.</param>
		private void OnPacketReceived(OscPacket packet)
		{
			if (PacketReceived != null)
			{
				PacketReceived(this, new OscPacketReceivedEventArgs(packet));
			}
		}

		/// <summary>
		/// Raises the BundleReceivedEvent.
		/// </summary>
		/// <param name="bundle">The packet to include in the event arguments.</param>
		private void OnBundleReceived(OscBundle bundle)
		{
			if (BundleReceived != null)
			{
				BundleReceived(this, new OscBundleReceivedEventArgs(bundle));
			}
		}

		/// <summary>
		/// Raises the MessageReceivedEvent.
		/// </summary>
		/// <param name="message">The message to include in the event arguments.</param>
		private void OnMessageReceived(OscMessage message)
		{
			if (MessageReceived != null)
			{
				MessageReceived(this, new OscMessageReceivedEventArgs(message));
			}
		}

		#endregion

        private TransportType mTransportType;
		private IPAddress mIPAddress;
		private int mPort;
        private IPEndPoint mIPEndPoint;
		private IPAddress mMulticastAddress;
		private UdpServer mUdpServer;
        //private TcpServer mTcpServer;
        private Thread mTcpServerThread;
		private List<string> mRegisteredMethods;
		private bool mFilterRegisteredMethods;
		private TransmissionType mTransmissionType;
		private volatile bool mHandleMessages;		
	}
}
