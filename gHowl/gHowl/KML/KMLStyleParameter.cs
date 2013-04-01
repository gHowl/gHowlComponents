using System;
using Grasshopper.Kernel;


namespace gHowl.KML
{
    public class KMLStyleParameter : GH_PersistentParam<KMLStyleType>
    {
        public KMLStyleParameter() : base("KML Style Parameter","Style","Defines Fill Color, Line Style, and Line Width","gHowl","KML") { }
       
        protected override GH_GetterResult Prompt_Plural(ref System.Collections.Generic.List<KMLStyleType> values)
        {
            values = new System.Collections.Generic.List<KMLStyleType>();
           // if (this.Prompt_ManageCollection(values))
          //  {
                return GH_GetterResult.success;
          //  }
          //  return GH_GetterResult.cancel;

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
            get { return new Guid("{bfc54567-2d1d-499f-b6c2-aa396397d792}"); }
        }
    }
}
