using System;
using Grasshopper.Kernel;


namespace gHowl.KML
{
    public class KMLStyleParameter : GH_PersistentParam<KMLStyleType>
    {
        public KMLStyleParameter() : base("KML Style Parameter","Style","Defines Fill Color, Line Style, and Line Width","gHowl","KML") { }
       
        protected override GH_GetterResult Prompt_Plural(ref System.Collections.Generic.List<KMLStyleType> values)
        {
            if (this.Prompt_ManageCollection(values))
            {
                return GH_GetterResult.success;
            }
            return GH_GetterResult.cancel;

        }

        protected override GH_GetterResult Prompt_Singular(ref KMLStyleType value)
        {
            value = new KMLStyleType();
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
