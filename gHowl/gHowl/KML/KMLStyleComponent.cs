using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using System.Drawing;
using gHowl.Properties;


namespace gHowl.KML
{
    public class KMLStyleComponent : GH_Component
    {
        public KMLStyleComponent() : base("KML Style","S","KML Object Attributes: Fill Color, Line Color, Line Width, Object Name","gHowl","KML") { }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddColourParameter("Fill Color", "F", "The fill color for polygon objects", GH_ParamAccess.item, Color.White);
            pManager.AddColourParameter("Curve Color", "L", "The line color for curve objects", GH_ParamAccess.item, Color.Black);
            pManager.AddNumberParameter("Curve Width", "W", "Line Width", GH_ParamAccess.item, 1.0);
            pManager.AddTextParameter("Name","N","Object Name",GH_ParamAccess.item,"gHowl");

            pManager[0].Optional = true;
            pManager[1].Optional = true;
            pManager[2].Optional = true;
            pManager[3].Optional = true;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("KML Style", "S", "KML Object Attributes");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            Color fc, lc = new Color();
            fc = Color.White;
            lc = Color.Black;
            double w=0;
            string n = "";
            if (!DA.GetData(0, ref fc) || !DA.GetData(1, ref lc) || !DA.GetData(2, ref w) || !DA.GetData(3, ref n)) { return; }
            KMLStyleType style = new KMLStyleType(fc,lc,w,n);
            DA.SetData(0, style);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{26114b99-80c2-4e9e-91d3-a17ac8ed800d}"); }
        }

        protected override Bitmap Icon
        {
            get
            {
                return Resources.KMLAttributes;
            }
        }
    }
}
