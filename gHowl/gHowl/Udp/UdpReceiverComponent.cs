using System;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using gHowl.Properties;
using Grasshopper;
using System.Text;
using System.Collections.Generic;

#if WITH_OSC
using Bespoke.Common.Osc;
#endif

namespace gHowl.Udp
{
    public class UdpReceiverComponent : SafeComponent
    {

        //The main disposable item
        private UdpClient _udpClient;
        private IPEndPoint _ip;
        private bool _isReceiving = false;

        private bool _messageReceived = false;
        private object[] _networkText = null;
        private string _serviceMessage = null;
        ReceivePattern _pattern = ReceivePattern.Text;


        public UdpReceiverComponent()
            : base("UDP Receiver", ">UDP<", "Allows to receive data on the network", "gHowl", "UDP")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_StringParam("Address", "@", "The local IP address to listen at. If empty, the loopback address is used", GH_ParamAccess.tree);
            pManager[0].Optional = true;

            pManager.Register_IntegerParam("Port", "P", "The port number to listen at", 6002, GH_ParamAccess.tree);
            pManager[1].Optional = true;

            pManager.Register_IntegerParam("Pattern", "#", @"The pattern:
0 = Parse received bytes as text
", 0, GH_ParamAccess.tree);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Info", "I", "Network information");
            pManager.Register_StringParam("Data", "D", "The string which was just received");
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }

        protected override void SolveInstance(IGH_DataAccess DA, bool secondOrLaterRun)
        {
          
            GH_Structure<GH_String> addrs;
            GH_Structure<GH_Integer> ports;
            GH_Structure<GH_Integer> patterns;

            if ((!DA.GetDataTree(0, out addrs)) ||
                (!DA.GetDataTree(1, out ports)) || ports == null || ports.IsEmpty ||
                (!DA.GetDataTree(2, out patterns)) || patterns == null || patterns.IsEmpty
                 )
            {
                StopReceiving();
                return;
            }
            if (addrs.PathCount > 1 || addrs.DataCount > 1 ||
                ports.PathCount != 1 || ports.DataCount != 1 ||
                patterns.PathCount != 1 || patterns.DataCount != 1
                 )
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Only one port and one address per Owl are allowed. The first items of any list or tree will be used.");
            }
            
            GH_Integer port = ports.get_FirstItem(false);
            GH_String address = addrs.get_FirstItem(false);
            GH_Integer pattern = patterns.get_FirstItem(false);
            IPAddress parsedAdd;
            _serviceMessage = "";
            if (address == null)
            {
                parsedAdd = IPAddress.Loopback;
            }
            else if (!IPAddress.TryParse(address.Value, out parsedAdd))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "This address is misspelled.");
                return;
            }

            if (pattern.Value != (int)_pattern){

                if (Enum.IsDefined(typeof(ReceivePattern), pattern.Value))
                {
                    _pattern = (ReceivePattern)pattern.Value;
                }
                else
                {
                    _serviceMessage += "Pattern is not defined\n";
                }
            }
            

            if (_isReceiving)
            {
                if (_messageReceived)
                {
                    //We set that message was collected
                    _serviceMessage += "A new message!\n";
                    _messageReceived = false;
                }
                else
                {
                    _serviceMessage += "No new message...\n";
                }

                if (_ip == null || port.Value != _ip.Port ||  !_ip.Address.Equals(parsedAdd))
                {
                    if (port.Value < 1)
                    {
                        StopReceiving();
                    }
                    else
                    {
                        StopReceiving();
                        if (ReceiveMessages(parsedAdd, port.Value))
                        {
                            _serviceMessage += string.Format("Switch to port {1} was successful\n", _ip.Address, _ip.Port);
                        }
                    }
                }
            }
            else
            { //Otherwise we need to start receiving...
                if (port.Value > 0)
                {
                    ReceiveMessages(parsedAdd, port.Value);
                }
                else
                {
                    _serviceMessage += "Waiting for a port to listen to...\n";
                }
            }

            DA.SetData(0, _serviceMessage);
            if (_networkText != null)
                DA.SetDataList(1, _networkText);
        }

        protected override void OnLockedChanged(bool nowIsLocked)
        {
            if (nowIsLocked)
                StopReceiving();
            base.OnLockedChanged(nowIsLocked);
        }


        /// <summary>
        /// This method is called when some packets are received
        /// </summary>
        /// <param name="ar">The result</param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                if (_isReceiving && ((ClientIpTuple)ar.AsyncState).Client == _udpClient)
                {
                    
                    IPEndPoint tempIp = ((ClientIpTuple)ar.AsyncState).Ip;
                    byte[] receiveBytes = ((ClientIpTuple)ar.AsyncState).Client.EndReceive(ar, ref tempIp);

                    if(_pattern == ReceivePattern.Text){
                    
                        _networkText = new object[] { Encoding.ASCII.GetString(receiveBytes) };
                        _messageReceived = true;

                    }
                   else if (_pattern == ReceivePattern.Bytes)
                    {

                        _networkText = new object[receiveBytes.Length];
                        for (int i = 0; i < receiveBytes.Length; i++)
                            _networkText[i] = receiveBytes[i];

                        _messageReceived = true;



                    }

#if WITH_OSC
                    else if (_pattern == ReceivePattern.OSC)
                    {

                        OscPacket msg = OscPacket.FromByteArray(tempIp, receiveBytes);
                        if (msg.IsBundle)
                        {
                            List<Object> list = new List<Object>();
                            for (int i = 0; i < msg.Data.Length; i++)
                            {
                                OscMessage bMsg = (OscMessage)msg.Data[i];
                                list.Add(bMsg.Address);
                                for (int j = 0; j < bMsg.Data.Length; j++)
                                {
                                    list.Add(bMsg.Data[j]);
                                }
                            }

                            Object[] tosend = new Object[list.Count];
                            list.CopyTo(tosend);
                            _networkText = tosend;
                            _messageReceived = true;
                        }
                        else
                        {
                            Object[] tempAdd = new Object[msg.Data.Length + 1];
                            tempAdd[0] = msg.Address;
                            Array.Copy(msg.Data, 0, tempAdd, 1, msg.Data.Length);
                            _networkText = tempAdd;
                            _messageReceived = true;
                        }

                    }
#endif


                    if (_udpClient != null && _ip != null)
                        _udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), new ClientIpTuple(_udpClient, _ip));
                }
            }
            catch (Exception ex)
            {
                _serviceMessage = "Error in ReceiveCallback: " + ex.Message + "\n\nLine: " + ex.StackTrace;
            }

            if (_doc.SolutionState != GH_ProcessStep.Process && _udpClient != null && !_askingNewSolution)
            {
                GH_InstanceServer.DocumentEditor.BeginInvoke((Action)delegate() {
                    if (_doc.SolutionState != GH_ProcessStep.Process)
                    {
                        _askingNewSolution = true;
                        this.ExpireSolution(true);
                        _askingNewSolution = false;
                    }
                });
            }
        }

        /// <summary>
        /// This helps preventing an external program that sends packets too frequently 
        /// from filling Grasshopper's to-do event list.
        /// </summary>
        private static bool _askingNewSolution = false;

        /// <summary>
        /// The starter of the receiving ping-pong. Start it only once when the client is not listening
        /// </summary>
        /// <param name="ipAddress">A string with the IP</param>
        /// <param name="port">An integer with the port #. Must be > 0. Some numbers are also invalid</param>
        /// <returns>Success (true) or not</returns>
        private bool ReceiveMessages(IPAddress ipAddress, int port)
        {
            try
            {


                _ip = new IPEndPoint(ipAddress, port); //Change here the IP address
                if (UdpHelper.IsMulticast(ipAddress))
                {
                    _udpClient = new UdpClient(port);
                    _udpClient.JoinMulticastGroup(_ip.Address);
                }
                else
                {

                    _udpClient = new UdpClient(_ip);

                }

                _udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), new ClientIpTuple(_udpClient, _ip));
                _isReceiving = true;
                _serviceMessage = string.Format("Listening for {0} messages at {1}, port {2}", IsLoopbackAddress(ipAddress) ? "local" : "external",
                    IsLoopbackAddress(ipAddress) ? "loopback" : ipAddress.ToString(), port.ToString());
                return true;

            }
            catch (SocketException ex)
            {

                if (ex.SocketErrorCode == SocketError.AddressAlreadyInUse)
                {
                    _serviceMessage = "AddressAlreadyInUse: you might have opened this socket on another program or another instance of Rhino.\n\n" + ex.Message + ".";
                }
                else if (ex.SocketErrorCode == SocketError.AddressNotAvailable)
                {
                    _serviceMessage = "AddressNotAvailable: this address is not a local address that can be monitored.\n\n" + ex.Message + ".";
                }
                else
                {
                    _serviceMessage = string.Format("({0}): {1}, {2}", ex.GetType().Name, ex.Message, ex.SocketErrorCode.ToString());
                }

            }
            catch (Exception ex)
            {
                _serviceMessage = "Error in receiveMessages: " + ex.Message;
            }

            return false;
        }
        public static bool IsLoopbackAddress(IPAddress address){
            return (address == IPAddress.Loopback ||
                address == IPAddress.IPv6Loopback);
        }

        /// <summary>
        /// The stopper of the receiving ping-pong. Call it only once when client is listening
        /// </summary>
        private void StopReceiving()
        {
            try
            {
                if (_isReceiving)
                {
                    _isReceiving = false;

                    if (_udpClient != null)
                    {
                        _udpClient.Client.Shutdown(SocketShutdown.Both);
                        _udpClient.Close();
                        _udpClient = null;
                    }
                }
                _serviceMessage = "Stopped receiving";
            }
            catch (Exception ex)
            {
                _serviceMessage = "Error while stopping receiving: " + ex.Message;
            }
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{5A093912-7A24-39E7-810C-797A143F3912}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    StopReceiving();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Dispose");
                }
            }
            else
            {
                _udpClient = null;
                _ip = null;
                _networkText = null;
                _serviceMessage = null;
            }
            base.Dispose(disposing);
        }

        protected override Bitmap Icon
        {
            get
            {
                return Resources.udp_receive;
            }
        }
    }
}
