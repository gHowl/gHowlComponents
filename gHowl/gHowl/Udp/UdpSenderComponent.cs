using System;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using gHowl.Properties;
#if WITH_OSC
using Bespoke.Common.Osc;
#endif

namespace gHowl.Udp
{
    public class UdpSenderComponent : SafeComponent
    {

        //The main disposable item
        private UdpClient _udpClient;

        private IPEndPoint _receiverIp;
        private byte[] _message;

        private string _serviceMessage = null;
        private Formatter _formatter = Formatter.Instance;
        SendPattern _pattern = SendPattern.Text;

        int _prt = 0;

        public UdpSenderComponent()
            : base("UDP Sender", "<UDP>", "Allows to send data through the network to any computer", "gHowl", "UDP")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_StringParam("Address", "@", "The IP address to send to. If empty, the loopback address is used", GH_ParamAccess.tree);
            pManager[0].Optional = true;

            pManager.Register_IntegerParam("Port", "P", "The port number to sent to, on the IP above", 6001, GH_ParamAccess.tree);
            pManager[1].Optional = true;

           
            pManager.Register_IntegerParam("Pattern", "#", @"The pattern:
  0 = sends data as ASCII text (7),
 10 = sends data as array of doubles (8)
", 0, GH_ParamAccess.tree);
            pManager[2].Optional = true;

            pManager.Register_GenericParam("Data", "D", "The string to send", GH_ParamAccess.list);
            pManager[3].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Info", "I", "Network information");
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.secondary; }
        }


        protected override void Initialize(int aliveElements)
        {
            StartSending();
            base.Initialize(aliveElements);
        }

        protected override void SolveInstance(IGH_DataAccess DA, bool secondOrLaterRun)
        {
            String addy = IPAddress.Loopback.ToString();
            GH_Structure<GH_String> addrs;
            GH_Structure<GH_Integer> ports;
            GH_Structure<GH_Integer> patterns;
            if ((!DA.GetDataTree(0, out addrs)) ||
                (!DA.GetDataTree(1, out ports)) || ports == null || ports.IsEmpty ||
                (!DA.GetDataTree(2, out patterns)) || patterns == null || patterns.IsEmpty
                 )
            {
                StopSending();
                return;
            }
            if (addrs.PathCount > 1 || addrs.DataCount > 1 ||
                ports.PathCount != 1 || ports.DataCount != 1 ||
                patterns.PathCount != 1 || patterns.DataCount != 1)
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Only one port and one address per Owl are allowed. The first items of any list or tree will be used.");
            }

            GH_Integer port = ports.get_FirstItem(false);
            GH_Integer pattern = patterns.get_FirstItem(false);
            _prt = port.Value;
            if (port.Value < 0)
                return;

            GH_String address = addrs.get_FirstItem(false);
            IPAddress parsedAdd;
            
            if (address == null)
            {
                parsedAdd = IPAddress.Loopback;
            }
            else if (!IPAddress.TryParse(address.Value, out parsedAdd))
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "This address is misspelled.");
                return;
            }

            if (pattern.Value != (int)_pattern)
            {
                if (Enum.IsDefined(typeof(SendPattern), pattern.Value))
                {
                    _pattern = (SendPattern)pattern.Value;
                }
                else
                {
                    _serviceMessage += "Pattern is not defined\n";
                }
            }

            if (_receiverIp == null || port.Value != _receiverIp.Port || parsedAdd != _receiverIp.Address)
            {
                _receiverIp = new IPEndPoint(parsedAdd, port.Value); //destination
            }

            switch(_pattern){

                case SendPattern.Text:
                    List<string> sMessage = new List<string>();
                    DA.GetDataList<string>(3, sMessage);
                    _message = _formatter.AsciiBytes(sMessage);
                    break;

                case SendPattern.DoubleArray:
                    List<double> nMessage = new List<double>();
                    DA.GetDataList<double>(3, nMessage);
                    _message = _formatter.DoubleArray(nMessage, _message);
                    break;

#if WITH_OSC
                case SendPattern.OSC:
                    List<string> oscMessage = new List<string>();
                    DA.GetDataList<string>(3, oscMessage);
                    _message = BytesForOSCMessage(oscMessage);
                    break;
#endif

                default:
                    _message = new byte[] { byte.MinValue, byte.MaxValue};
                    break;
            }

            if (SendMessages(_message, _message.Length, _receiverIp))
            {
                _serviceMessage = string.Format("Sending successful - UDP does not guarantee any arrival");
            }

            DA.SetData(0, "Data sent");
        }

#if WITH_OSC
        private byte[] BytesForOSCMessage(List<string> oscMessage)
        {
            string oscAddress = "/GH/none"; //This is what was happening before. 

            if (oscMessage.Count > 1)
            {
                oscAddress = oscMessage[0];
            }

            OscMessage msg = new OscMessage(new IPEndPoint(IPAddress.Loopback, _prt), oscAddress); //alive method is the address, the last part is the value

            for (int j = 1; j < oscMessage.Count; j++)
            {
                msg.Append(oscMessage[j]);
            }

            return msg.ToByteArray();
        }
#endif

        private bool SendMessages(byte[] _message, int length, IPEndPoint endpoint)
        {
            return length == _udpClient.Send(_message, length, endpoint);
        }

        private void StartSending()
        {
            if (NetworkInterface.GetIsNetworkAvailable())
                _udpClient = new UdpClient();
        }

        private void StopSending()
        {
            _udpClient.Close();
            _udpClient = null;
        }

        protected override void OnLockedChanged(bool nowIsLocked)
        {
            if(nowIsLocked)
                StopSending();
            base.OnLockedChanged(nowIsLocked);
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{E7B66191-6548-0CE3-7D16-1EC3813EC8D1}");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                try
                {
                    StopSending();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Dispose");
                }
            }

            base.Dispose(disposing);
        }

        protected override Bitmap Icon
        {
            get
            {
                return Resources.udp_send;
            }
        }
    }
}
