using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using Rhino.Geometry;
using System.Drawing;
using gHowl.Properties;
using Rhino.DocObjects;

namespace gHowl.Geo
{
    public class XYZtoGPS : GH_Component
    {
        public XYZtoGPS() : base("XYZ to Geo", "XYZ->Geo", "Map XYZ Coordinates to WSG84", "gHowl", "Geo") { }
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PointParam("XYZ_IN", "P", "Points in model to convert to WSG84 coordinates", GH_ParamAccess.tree);
            pManager.Register_PointParam("EAP_XYZ", "XYZ", "Earth Anchor Point Model Coordinates", GH_ParamAccess.item);
            pManager.Register_PointParam("EAP_Geo", "Geo", "Earth Anchor Point Coordinates in D.D. Longitude, Latitude, Altitude", GH_ParamAccess.item);
            pManager[0].Optional = false;
            pManager[1].Optional = false;
            pManager[2].Optional = false;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_PointParam("Geo Points", "P", "XYZ Points converted to WSG84 Coordinates");
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("7251a345-6b2e-4d21-bdb8-3b26176b21d8"); }
        }

        protected override Bitmap Icon
        {
            get
            {
                return Resources.worldEdit;
                
            }
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            GH_Structure<GH_Point> ptXYZ, ptTree = new GH_Structure<GH_Point>();
            Point3d EAP_GPS = new Point3d();
            Point3d EAP_XYZ = new Point3d();
            if ((!DA.GetDataTree(0, out ptXYZ)) || ptXYZ == null || ptXYZ.IsEmpty ||
                (!DA.GetData(1, ref EAP_XYZ)) || EAP_XYZ == null ||
                (!DA.GetData(2, ref EAP_GPS)) || EAP_GPS == null
            )
            { return; }


            //Set up EAP 
            EarthAnchorPoint eap = new EarthAnchorPoint();
            eap.EarthBasepointLongitude = EAP_GPS.X;
            eap.EarthBasepointLatitude = EAP_GPS.Y;
            eap.EarthBasepointElevation = EAP_GPS.Z;
            eap.ModelBasePoint = EAP_XYZ;    
            Rhino.UnitSystem us = new Rhino.UnitSystem();
            Transform xf = eap.GetModelToEarthTransform(us);

            for (int i = 0; i < ptXYZ.PathCount; i++)
            {

                for (int j = 0; j < ptXYZ.get_Branch(i).Count; j++)
                {
                    Point3d pt = ptXYZ.get_DataItem(ptXYZ.Paths[i], j).Value;
                    if (pt != null)
                    {
                        Point3d ptON = new Point3d(pt.X, pt.Y, pt.Z);
                        ptON = xf * ptON; //where the magic happens
                        pt.X = ptON.X;
                        pt.Y = ptON.Y;
                        pt.Z = ptON.Z;
                        GH_Point ptOut = new GH_Point(pt);
                        ptTree.Append(ptOut, ptXYZ.Paths[i]);
                    }

                }
            }
            DA.SetDataTree(0, ptTree);

        }


    }
}
