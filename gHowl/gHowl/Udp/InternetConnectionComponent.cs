using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace gHowl.Udp
{
    public class InternetConnectionComponent : GH_Component
    {
        Formatter speedFormatter = Formatter.Instance;

        public InternetConnectionComponent()
            : base("Network Source", "NetSource", "Discovers an external internet connection, and retrieve name and properties if one is available", "gHowl", "UDP")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_BooleanParam("Network adapter is up", "N", "Indicates if an external internet connection is available");
            pManager.Register_StringParam("Adapter properties", "S", @"Retrieves properties from the first available connection provider:

[0] iterface name
[1] adapter description
[2] type
[3] maximum theoretical speed (human readable)
[4] speed (long as string)
[5] MAC address
[6] IPv4
[7] IPv6
[8] bytes received
[9] bytes sent
");
            pManager.Register_StringParam("Local Ip Address", "@", "The suggested local IP address to use, visualized in its standard representation");
        }
        public override GH_Exposure Exposure
        {
            get { return GH_Exposure.primary; }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<string[]> details;
            int connections = AdapterDiscoverer.InternetAvailable(out details);

            DA.SetData(0, connections > 0);

            GH_Structure<GH_String> properties = new GH_Structure<GH_String>();

            for (int i = 0; i < details.Count; i++)
            {
                string[] detail = details[i];
                GH_Path path = new GH_Path(0, i);
                properties.Append(new GH_String(detail[0]), path);
                properties.Append(new GH_String(detail[1]), path);
                properties.Append(new GH_String(detail[2]), path);
                properties.Append(new GH_String(detail[3]), path);
                properties.Append(new GH_String(detail[4]), path);
                properties.Append(new GH_String(detail[5]), path);
                properties.Append(new GH_String(detail[6]), path);
                properties.Append(new GH_String(detail[7]), path);
                properties.Append(new GH_String(detail[8]), path);
                properties.Append(new GH_String(detail[9]), path);
            }
            DA.SetDataTree(1, properties);

            if (connections > 0)
            {
                //Sets the first available adapter IPv4, if that is null then the first IPv6, otherwise the loopback
                DA.SetData(2, (details[0][6] != null) ? details[0][6].ToString() : (details[0][7] != null ? details[0][7].ToString() : IPAddress.Loopback.ToString()));
            }
            else
            {
                DA.SetData(2, IPAddress.Loopback.ToString());
            }
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{1A578ADA-547D-0931-C12B-04410D154353}");
            }
        }

        protected override Bitmap Icon
        {
            get
            {
                return gHowl.Properties.Resources.adapter;
            }
        }
    }
}
