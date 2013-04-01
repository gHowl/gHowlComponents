using System;
using Grasshopper.Kernel;


namespace gHowl.KML
{
    public class KMLStyleParameter_OBSOLETE : GH_PersistentParam<KMLStyleType_OBSOLETE>
    {
        public KMLStyleParameter_OBSOLETE() : base("KML Style Parameter","Style","Defines Fill Color, Line Style, and Line Width","gHowl","KML") { }
       
        protected override GH_GetterResult Prompt_Plural(ref System.Collections.Generic.List<KMLStyleType_OBSOLETE> values)
        {
            values = new System.Collections.Generic.List<KMLStyleType_OBSOLETE>();
           // if (this.Prompt_ManageCollection(values))
          //  {
                return GH_GetterResult.success;
          //  }
          //  return GH_GetterResult.cancel;

        }

        protected override GH_GetterResult Prompt_Singular(ref KMLStyleType_OBSOLETE value)
        {
            value = new KMLStyleType_OBSOLETE();
            return GH_GetterResult.success;

        }

        public override GH_Exposure Exposure
        {
            get{ return GH_Exposure.hidden; }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{3c1cc17a-a426-46ec-8ee3-ca152185f46b}"); }
        }
    }
}
