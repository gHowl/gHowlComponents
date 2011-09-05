using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using System.Drawing;
using gHowl.Properties;


namespace gHowl.KML
{
    public class KMLStyleComponent : GH_Component
    {
        public KMLStyleComponent() : base("KML Style","S","KML Object Attributes: Fill Color, Line Color, Line Width","gHowl","KML") { }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_ColourParam("Fill Color", "F", "The fill color for polygon objects", Color.White, GH_ParamAccess.item);
            pManager.Register_ColourParam("Curve Color", "L", "The line color for curve objects", Color.Black, GH_ParamAccess.item);
            pManager.Register_DoubleParam("Curve Width", "W", "Line Width",1.0, GH_ParamAccess.item);
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
            if (!DA.GetData(0, ref fc) || !DA.GetData(1, ref lc) || !DA.GetData(2, ref w)) { return; }
            KMLStyleType style = new KMLStyleType(fc,lc,w);
            DA.SetData(0, style);
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{8354046b-989b-4d61-a92d-e109bb53dad3}"); }
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
