using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Rhino.Geometry;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using System.Drawing;
using gHowl.Properties;

namespace gHowl.Geo
{
   public class GeotoXYZ : GH_Component
    {
       public GeotoXYZ() : base("Geo to XYZ", "Geo->XYZ", "Map WSG84 Coordinates to XYZ", "gHowl", "GEO") { }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PointParam("Geo_IN","P","WSG84 Coordinates as Decimal Degrees Longitude, Latitude, Altitude",GH_ParamAccess.tree);

            pManager.Register_PointParam("Pt 1 Reference XYZ","P1_XYZ","First Reference Point XYZ, usually lower left corner of mapping area",GH_ParamAccess.item);
            pManager.Register_PointParam("Pt 2 Reference XYZ", "P2_XYZ", "Second Reference Point XYZ, usually upper right corner of mapping area", GH_ParamAccess.item);
            pManager.Register_PointParam("Pt 1 Reference Geo", "P1_Geo", "First Reference Point WSG84, usually lower left corner of mapping area", GH_ParamAccess.item);
            pManager.Register_PointParam("Pt 2 Reference Geo", "P2_Geo", "Second Reference Point WSG84, usually upper right corner of mapping area", GH_ParamAccess.item);
            pManager[0].Optional = false;
            pManager[1].Optional = false;
            pManager[2].Optional = false;
            pManager[3].Optional = false;
            pManager[4].Optional = false;
      
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("XYZ Points", "P", "WSG84 Coordinates in XYZ");
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("6268ae44-0782-40ca-ac94-21c62ca7943d"); }
        }
        protected override Bitmap Icon
        {
            get
            {
                return Resources.xyz;
            }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Point> ptGPS, ptTree = new GH_Structure<GH_Point>();
            Point3d pt1_GPS = new Point3d();
            Point3d pt2_GPS = new Point3d();
            Point3d pt1_XYZ= new Point3d();
            Point3d pt2_XYZ = new Point3d();

            if ((!DA.GetDataTree(0, out ptGPS)) || ptGPS == null || ptGPS.IsEmpty ||
                (!DA.GetData(1, ref pt1_XYZ)) || pt1_XYZ == null ||
                (!DA.GetData(2, ref pt2_XYZ)) || pt2_XYZ == null ||
                (!DA.GetData(3, ref pt1_GPS)) || pt1_GPS == null ||
                (!DA.GetData(4, ref pt2_GPS)) || pt2_GPS == null 
                ) 
            { return; }

            Interval domLon = new Interval(pt1_GPS.X, pt2_GPS.X);
            Interval domLat = new Interval(pt1_GPS.Y, pt2_GPS.Y);
            Interval domX = new Interval(pt1_XYZ.X, pt2_XYZ.X);
            Interval domY = new Interval(pt1_XYZ.Y, pt2_XYZ.Y);

            for (int i = 0; i < ptGPS.PathCount; i++)
            {

                for (int j = 0; j < ptGPS.get_Branch(i).Count; j++)
                {
                    
                    Point3d pt = ptGPS.get_DataItem(ptGPS.Paths[i], j).Value;
                    if (pt != null)
                    {
                        Point3d ptXYZ = new Point3d(reMap(pt.X, domLon, domX), reMap(pt.Y, domLat, domY), pt.Z);
                        GH_Point ptOut = new GH_Point(ptXYZ);
                        ptTree.Append(ptOut, ptGPS.Paths[i]);
                    }
                  
                }
            }

            DA.SetDataTree(0, ptTree);
        }

        public double reMap(double val, Interval dom, Interval domDest)
        {
            return ((((val - dom.T0) / (dom.Length)) * (domDest.Length)) + domDest.T0);
        }
        
    }
}
