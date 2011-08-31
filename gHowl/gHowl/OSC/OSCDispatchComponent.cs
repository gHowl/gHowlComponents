using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;

namespace gHowl.Udp
{
    public class OSCDispatchComponent: GH_Component
    {
        String[] _o;
        String _oscDev;

        public OSCDispatchComponent() : base("OSC Dispatch", "OSC_D", "Store OSC data from multiple sources", "gHowl", "UDP") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_StringParam("OSC Devices", "D", "The OSC devices to listen to", GH_ParamAccess.list);
            pManager.Register_StringParam("OSC Message", "M", "The incoming OSC Message", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("OSC Device", "D", "The name of the device sending data");
            pManager.Register_StringParam("OSC Data","O","The stored data related to the device name");
        }

        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<String> OSCMsg = new List<String>();
            List<String> OSCDevices = new List<String>();
            if ((!DA.GetDataList(0, OSCDevices)) ||
                (!DA.GetDataList(1, OSCMsg))
                ) { }
            DA.SetData(0, _oscDev);
            Array.Resize(ref _o, OSCDevices.Count);
            updateData(OSCMsg,OSCDevices);
            DA.SetDataList(1, _o);

        }

        public void updateData(List<string> OSCMsg, List<string> OSCDevices)
        {

            for (int i = 0; i < OSCDevices.Count; i++)
            {
                if (OSCMsg[0] == OSCDevices[i])
                {
                    _oscDev = OSCMsg[0];

                    if (OSCMsg.Count > 2)
                    {
                        StringBuilder sb = new StringBuilder();
                        sb.Append(OSCMsg[1]);

                        for (int j = 2; j < OSCMsg.Count; j++)
                        {
                            sb.Append(",");
                            sb.Append(OSCMsg[j]);
                        }
                        _o[i] = sb.ToString();
                    }
                    else
                    {
                        _o[i] = OSCMsg[1];
                    }
                    return;
                }
                else
                {
                    _oscDev = "Device not listed";
                }
            }
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return gHowl.Properties.Resources.OSCDispatch;
            }
        }
        public override Guid ComponentGuid
        {
             get
            {
                return new Guid("6b8c8c86-b384-452a-b3e2-4fa9ad693476");
            }
        }
    }
}
