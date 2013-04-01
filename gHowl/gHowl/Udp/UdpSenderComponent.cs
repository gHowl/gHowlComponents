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
using Bespoke.Common.Osc;


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
        private bool patternChanged = false;

        int _prt = 0;

        public UdpSenderComponent()
            : base("UDP Sender", "<UDP>", "Allows to send data through the network to any computer", "gHowl", "UDP")
        {
            this.Message = "UDP";
            this.SetValue("UDP", true);
            this.SetValue("OSC", false);
            this.ValuesChanged();
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddTextParameter("Address", "@", "The IP address to send to. If empty, the loopback address is used", GH_ParamAccess.tree);
            pManager[0].Optional = true;

            pManager.AddIntegerParameter("Port", "P", "The port number to sent to, on the IP above", GH_ParamAccess.tree, 6001);
            pManager[1].Optional = true;
/*
           
            pManager.AddIntegerParameter("Pattern", "#", @"The pattern:
  0 = sends data as ASCII text (7),
 10 = sends data as array of doubles (8)
999 = Sends OSC Message
",GH_ParamAccess.tree, 0);
            pManager[2].Optional = true;
            */
            pManager.AddGenericParameter("Data", "D", "The string to send", GH_ParamAccess.list);
            pManager[2].Optional = true;

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
           // GH_Structure<GH_Integer> patterns;
            if ((!DA.GetDataTree(0, out addrs)) ||
                (!DA.GetDataTree(1, out ports)) || ports == null || ports.IsEmpty 
                
                 )
            {
                StopSending();
                return;
            }
            if (addrs.PathCount > 1 || addrs.DataCount > 1 ||
                ports.PathCount != 1 || ports.DataCount != 1 )
                
            {
                this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Only one port and one address per Owl are allowed. The first items of any list or tree will be used.");
            }

            GH_Integer port = ports.get_FirstItem(false);
            //GH_Integer pattern = patterns.get_FirstItem(false);
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

            if (this.GetValue("UDP", false))
            {
                _pattern = SendPattern.Text;
            }
            else if (this.GetValue("OSC", false))
            {
                _pattern = SendPattern.OSC;
            }
/*
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
            */

            if (_receiverIp == null || port.Value != _receiverIp.Port || parsedAdd != _receiverIp.Address || patternChanged)
            {
                _receiverIp = new IPEndPoint(parsedAdd, port.Value); //destination
                patternChanged = false;
            }

            switch(_pattern){

                case SendPattern.Text:
                    List<string> sMessage = new List<string>();
                    DA.GetDataList<string>(2, sMessage);
                    _message = _formatter.AsciiBytes(sMessage);
                    break;

                case SendPattern.DoubleArray:
                    List<double> nMessage = new List<double>();
                    DA.GetDataList<double>(2, nMessage);
                    _message = _formatter.DoubleArray(nMessage, _message);
                    break;


                case SendPattern.OSC:
                    List<string> oscMessage = new List<string>();
                    DA.GetDataList<string>(2, oscMessage);
                    _message = BytesForOSCMessage(oscMessage);
                    break;


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
                return new Guid("{eca7f50b-a181-4509-824a-25538efab4b9}");
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


        public override void AppendAdditionalMenuItems(System.Windows.Forms.ToolStripDropDown menu)
        {
            base.AppendAdditionalMenuItems(menu);

            Menu_AppendItem(menu, "UDP", new EventHandler(this.Menu_UDPClicked), true, this.GetValue("UDP", true)).ToolTipText = "User Datagram Protocol";
            Menu_AppendItem(menu, "OSC", new EventHandler(this.Menu_OSCClicked), true, this.GetValue("OSC", false)).ToolTipText = "Open Sound Control";
            //Menu_AppendItem(menu, "Relative To Ground", new EventHandler(this.Menu_RelativeToGroundClicked), true, this.GetValue("RelativeToGround", false)).ToolTipText = "Interpret the altitude in meters relative to the terrain elevation.";
        }

        private void Menu_UDPClicked(object sender, EventArgs e)
        {
            //this.SetValue("Absolute", !this.GetValue("Absolute", false));
            this.SetValue("UDP", true);
            this.SetValue("OSC", false);
            

            this.ExpireSolution(true);
           // this.ClearData();
        }

        private void Menu_OSCClicked(object sender, EventArgs e)
        {
            //this.SetValue("ClampToGround", !this.GetValue("ClampToGround", false));
            this.SetValue("UDP", false);
            this.SetValue("OSC", true);
            this.ExpireSolution(true);
           // this.ClearData();
        }

        

        protected override void ValuesChanged()
        {
            if (this.GetValue("UDP", false))
            {
                this.Message = "UDP";
            }
            else if (this.GetValue("OSC", false))
            {
                this.Message = "OSC";
            }

            patternChanged = true;
        }

    }
}
