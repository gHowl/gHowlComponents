using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace Bespoke.Common.Net
{
	/// <summary>
	/// 
	/// </summary>
	/// <param name="sender"></param>
	/// <param name="e"></param>
	public delegate void UdpDataReceivedHandler(object sender, UdpDataReceivedEventArgs e);

    /// <summary>
    /// 
    /// </summary>
    public class UdpState
    {
        /// <summary>
        /// 
        /// </summary>
        public UdpClient Client
        {
            get
            {
                return mClient;
            }
            set
            {
                mClient = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public IPEndPoint IPEndPoint
        {
            get
            {
                return mIPEndPoint;
            }
            set
            {
                mIPEndPoint = value;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="client"></param>
        /// <param name="ipEndPoint"></param>
        public UdpState(UdpClient client, IPEndPoint ipEndPoint)
        {
            mClient = client;
            mIPEndPoint = ipEndPoint;
        }

        private UdpClient mClient;
        private IPEndPoint mIPEndPoint;
    }

	/// <summary>
	/// 
	/// </summary>
	public class UdpServer
	{
		#region Events

		/// <summary>
		/// 
		/// </summary>
		public event UdpDataReceivedHandler DataReceived;

		#endregion

		#region Properties

		/// <summary>
		/// 
		/// </summary>
		public IPAddress IPAddress
		{
			get
			{
				return mIPAddress;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public int Port
		{
			get
			{
				return mPort;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public IPAddress MulticastAddress
		{
			get
			{
				return mMulticastAddress;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsRunning
		{
			get
			{
				return mAcceptingConnections;
			}
		}

		/// <summary>
		/// 
		/// </summary>
		public TransmissionType TransmissionType
		{
			get
			{
				return mTransmissionType;
			}
		}

		#endregion

		/// <summary>
        /// Binds the server to the loopback address.
        /// </summary>
        /// <param name="port"></param>
        public UdpServer(int port)
            : this(IPAddress.Loopback, port, null, TransmissionType.LocalBroadcast)
        {
        }

		/// <summary>
		/// Binds the server to the loopback address.
		/// </summary>
		/// <param name="port"></param>
		/// <param name="multicastAddress"></param>
		public UdpServer(int port, IPAddress multicastAddress)
			: this(IPAddress.Loopback, port, multicastAddress, TransmissionType.Multicast)
		{
		}

		/// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
		public UdpServer(IPAddress ipAddress, int port)
			: this(ipAddress, port, null, TransmissionType.Unicast)
		{
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
		/// <param name="multicastAddress"></param>
		/// <param name="transmissionType"></param>
        public UdpServer(IPAddress ipAddress, int port, IPAddress multicastAddress, TransmissionType transmissionType)
        {
            mPort = port;
            mIPAddress = ipAddress;
			mTransmissionType = transmissionType;

			if (mTransmissionType == TransmissionType.Multicast)
			{
				Trace.Assert(multicastAddress != null);
				mMulticastAddress = multicastAddress;
			}

#if !WINDOWS
            mDataBuffer = new byte[DataBufferSize];
#endif

            mAcceptingConnections = true;
            mAsynCallback = new AsyncCallback(EndReceive);
        }

#if WINDOWS
		/// <summary>
		/// 
		/// </summary>
		/// <returns></returns>
		public static IPAddress[] GetLocalIPAddress()
		{
			IPAddress[] localAddresses = Dns.GetHostAddresses(Dns.GetHostName());
			if (localAddresses.Length == 0)
			{
				throw new Exception("No local IP Address address found.");
			}

			return localAddresses;
		}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static bool IsIPEndPointAvailable(IPAddress ipAddress, int port)
        {
            IPEndPoint ipEndPoint = new IPEndPoint(ipAddress, port);
            return IsIPEndPointAvailable(ipEndPoint);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ipEndPoint"></param>
        /// <returns></returns>
        public static bool IsIPEndPointAvailable(IPEndPoint ipEndPoint)
        {
            bool isIPEndPointAvailable = true;

            System.Net.NetworkInformation.IPGlobalProperties ipGlobalProperties = System.Net.NetworkInformation.IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] activeUdpListeners = ipGlobalProperties.GetActiveUdpListeners();
            foreach (IPEndPoint activeUdpListener in activeUdpListeners)
            {
                if ((activeUdpListener.Address == ipEndPoint.Address) && (activeUdpListener.Port == ipEndPoint.Port))
                {
                    isIPEndPointAvailable = false;
                    break;
                }
            }

            return isIPEndPointAvailable;
        }
#endif
        
        /// <summary>
		/// Start the UDP server and begin receiving data.
		/// </summary>
		public void Start()
		{
            IPEndPoint ipEndPoint;

            switch (mTransmissionType)
            {
                case TransmissionType.Unicast:
                    ipEndPoint = new IPEndPoint(mIPAddress, mPort);
                    mUdpClient = new UdpClient(ipEndPoint);
                    break;

                case TransmissionType.Multicast:
                    ipEndPoint = new IPEndPoint(IPAddress.Any, mPort);
                    mUdpClient = new UdpClient(ipEndPoint);
                    mUdpClient.JoinMulticastGroup(mMulticastAddress);
                    break;

                case TransmissionType.Broadcast:
                case TransmissionType.LocalBroadcast:
                    ipEndPoint = new IPEndPoint(IPAddress.Any, mPort);
                    mUdpClient = new UdpClient(ipEndPoint);
                    break;

                default:
                    throw new Exception();
            }

            UdpState udpState = new UdpState(mUdpClient, ipEndPoint);

            if (mAcceptingConnections)
            {
#if WINDOWS
                mUdpClient.BeginReceive(mAsynCallback, udpState);
#else
                mUdpClient.Client.BeginReceive(mDataBuffer, 0, mDataBuffer.Length, SocketFlags.None, mAsynCallback, udpState);
#endif
            }
		}

		/// <summary>
		/// Stop the UDP server.
		/// </summary>
		public void Stop()
		{
			mAcceptingConnections = false;

            if (mUdpClient != null)
            {
                if (mTransmissionType == TransmissionType.Multicast)
                {
                    mUdpClient.DropMulticastGroup(mMulticastAddress);
                }

                mUdpClient.Close();
            }
		}

		#region Private Methods

#if WINDOWS
        /// <summary>
        /// EndReceive paired call.
        /// </summary>
        /// <param name="asyncResult">Paired result object from the BeginReceive call.</param>
        private void EndReceive(IAsyncResult asyncResult)
        {
            try
            {
                UdpState udpState = (UdpState)asyncResult.AsyncState;
                UdpClient udpClient = udpState.Client;
                IPEndPoint ipEndPoint = udpState.IPEndPoint;

                byte[] data = udpClient.EndReceive(asyncResult, ref ipEndPoint);
                if (data != null && data.Length > 0)
                {
                    OnDataReceived(new UdpDataReceivedEventArgs(ipEndPoint, data));
                }

                if (mAcceptingConnections)
                {
                    udpClient.BeginReceive(mAsynCallback, udpState);
                }
            }
            catch (ObjectDisposedException)
            {
                // Suppress error
            }
        }
#else
         /// <summary>
        /// EndReceive paired call.
        /// </summary>
        /// <param name="asyncResult">Paired result object from the BeginReceive call.</param>
        private void EndReceive(IAsyncResult asyncResult)
        {
            try
            {
                UdpState udpState = (UdpState)asyncResult.AsyncState;
                Socket socket = udpState.Client.Client;
                IPEndPoint ipEndPoint = udpState.IPEndPoint;

                if (socket != null)
                {
                    int bytes = socket.EndReceive(asyncResult);
                    if (bytes > 0)
                    {
                        OnDataReceived(new UdpDataReceivedEventArgs(ipEndPoint, mDataBuffer));
                    }

                    if (mAcceptingConnections)
                    {
                        mUdpClient.Client.BeginReceive(mDataBuffer, 0, mDataBuffer.Length, SocketFlags.None, mAsynCallback, udpState);
                    }
                }
            }
            catch (ObjectDisposedException)
            {
                // Suppress error
            }
        }
#endif

		/// <summary>
		/// Raise the DataReceived event.
		/// </summary>
		/// <param name="e">An EventArgs object that contains the event data.</param>
		private void OnDataReceived(UdpDataReceivedEventArgs e)
		{
			if (DataReceived != null)
			{
				DataReceived(this, e);
			}
		}

		#endregion

        private static readonly int DataBufferSize = 65507;

		private IPAddress mIPAddress;
		private int mPort;
		private IPAddress mMulticastAddress;
		private TransmissionType mTransmissionType;
        private UdpClient mUdpClient;
        private AsyncCallback mAsynCallback;
        private byte[] mDataBuffer;
		private volatile bool mAcceptingConnections;
	}
}
