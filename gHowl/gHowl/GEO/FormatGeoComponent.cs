using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using Rhino.Geometry;
using System.Drawing;
using gHowl.Properties;

namespace gHowl.Geo
{
    public class FormatGeo : GH_Component
    {
        public FormatGeo() : base("Format Geo", "DMs -> DD", "Formats WSG84 coordinates", "gHowl", "GEO") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PointParam("GEO", "P", "Incoming WSG84 coordinates", GH_ParamAccess.tree);
            pManager[0].Optional = false;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("Formated Coordinates", "P", "The formated WSG84 coordinates");
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("6e6fb252-5974-4e39-b0ad-905304b2e7d8 "); }
        }
        protected override Bitmap Icon
        {
            get
            {
                return Resources.worldIcon;
            }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Point> ptTreeIn = new GH_Structure<GH_Point>();
           // IEnumerable<GH_Point> ptTreeIn_nn = new GH_Structure<GH_Point>();
            GH_Structure<GH_Point> ptTreeOut = new GH_Structure<GH_Point>();
            if ((!DA.GetDataTree(0, out ptTreeIn)) || ptTreeIn == null || ptTreeIn.IsEmpty) { return; }
            //ptTreeIn_nn = ptTreeIn.NonNulls;
            
            for (int i = 0; i < ptTreeIn.PathCount; i++)
            {

                for (int j = 0; j < ptTreeIn.get_Branch(i).Count; j++)
                {

                    Point3d ptGPS = ptTreeIn.get_DataItem(ptTreeIn.Paths[i], j).Value;
                    if(ptGPS != null){
                    string lon = ptGPS.X.ToString();
                    string lat = ptGPS.Y.ToString();
                    string[] lonData = lon.Split(new char[] { '.' });
                    string[] latData = lat.Split(new char[] { '.' });
                    Point3d ptGPSed = new Point3d(convertCoo(lonData), convertCoo(latData), ptGPS.Z);
                    GH_Point ptOut = new GH_Point(ptGPSed);
                    ptTreeOut.Append(ptOut, ptTreeIn.Paths[i]);
                    }
                }
            }

            DA.SetDataTree(0, ptTreeOut);

        }

        //functions
        public double convertCoo(string[] data)
        {
            string str1 = null;
            string str2 = null;
            double num = 0;
            //if (data[0] != null)
            // {


            if ((data[0].Length == 3))
            {

                str1 = data[0].Substring(0, 1);
                str2 = data[0].Substring(1, 2);
                if (data.Length > 1) { 
                str2 = str2 + "." + data[1];
                }
                num = Convert.ToDouble(str2) / 60;
                num = Convert.ToDouble(str1) + num;
                return num;
            }
            else if ((data[0].Length == 4))
            {

                str1 = data[0].Substring(0, 2);
                str2 = data[0].Substring(2, 2);
                if (data.Length > 1)
                {
                    str2 = str2 + "." + data[1];
                }
                num = Convert.ToDouble(str2) / 60;
                num = Convert.ToDouble(str1) + num;
                return num;
            }
            else if ((data[0].Length == 5))
            {

                str1 = data[0].Substring(0, 3);
                str2 = data[0].Substring(3, 2);
                if (data.Length > 1)
                {
                    str2 = str2 + "." + data[1];
                }
                num = Convert.ToDouble(str2) / 60;
                num = Convert.ToDouble(str1) + num;
                return num;
            }

            else if (data[0] == null)
            {

                return 0;
            }

            else
            {
                return 0;
            }
        }



    }
}
