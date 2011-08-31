using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;
using System.Windows.Forms;

namespace gHowl.Udp
{
    public class OSCChannelComponent: GH_Component
    {
      
        String tmpList;
        String _d;
        String _nn;

        public OSCChannelComponent() : base("OSC Channel", "OSC_C", "Store OSC data from a single source", "gHowl", "UDP") 
        {
           
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_StringParam("OSC Message", "M", "The incoming OSC Message", GH_ParamAccess.list);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("OSC Data","O","The stored data related to the device name (nickname)");
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.tertiary; }
        }
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            _nn = base.NickName;
            
            List<String> OSCMsg = new List<String>();
            if (!DA.GetDataList(0, OSCMsg)) { }
            updateSingleData(OSCMsg, _nn);
            DA.SetData(0, _d);
        }

        public void updateSingleData(List<String> OSCMsg, String OSCDevice)
        {
            if (OSCDevice == OSCMsg[0])
            {
                if (OSCMsg.Count > 2)
                {
                    for (int j = 1; j < OSCMsg.Count; j++)
                    {
                        if (j == 1)
                        {
                            tmpList = OSCMsg[j] + ",";
                        }
                        else if (j == OSCMsg.Count - 1)
                        {
                            tmpList = tmpList + OSCMsg[j];
                        }
                        else
                        {
                            tmpList = tmpList + OSCMsg[j] + ",";
                        }
                    }
                    _d = tmpList;
                    tmpList = "";
                }
                else
                {
                    _d = OSCMsg[1];
                }
            }
        }

        

        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return gHowl.Properties.Resources.OSCChannel;
            }
        }

        public override Guid ComponentGuid
        {
             get
            {
                return new Guid("4654efcd-8975-4fe8-90c2-d767ccdd80c4");
            }
        }
    }
}
