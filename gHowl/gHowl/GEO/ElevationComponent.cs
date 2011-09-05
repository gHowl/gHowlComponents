using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using Grasshopper.Kernel.Data;
using System.Text;
using System.IO;
using System.Xml;
using System.Drawing;
using gHowl.Properties;


namespace gHowl.Geo
{
    public class ElevationComponent : GH_Component
    {
        public ElevationComponent() : base("Get Elevation", "E", "Given WGS84 coordinates, this component will return the elevation(s)", "gHowl", "GEO") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_PointParam("Geo Points", "P", "WSG84 Decimal Degree formatted points", GH_ParamAccess.list);
            pManager[0].Optional = false;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_DoubleParam("Geo Points", "P", "Elevated Points");
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            List<GH_Point> inPts = new List<GH_Point>();
            if (!DA.GetDataList<GH_Point>(0,inPts)) { return; }
            int cnt = 0;
            string preUrl = "http://maps.googleapis.com/maps/api/elevation/xml?locations=";
            string postUrl = "&sensor=false";
            StringBuilder url = new StringBuilder();
            StringBuilder sb;
            url.Append(preUrl);
            List<double> outPts = new List<double>();
            

           

            for (int i = 0; i < inPts.Count; i++)
            {
                url.Append(inPts[i].Value.Y);
                url.Append(",");
                url.Append(inPts[i].Value.X);
                cnt++;

                //need a better way to throtle this condition
                //Elevations API URLs can only be a Maximum of 2048 characters before URL Encoding
                
                if (cnt == 50 || i == inPts.Count - 1) 
                {

                    url.Append(postUrl);
                    //Print((url.ToString().Length).ToString());

                    cnt = 0;
                    try
                    {
                        System.Net.HttpWebRequest HttpWReq = (System.Net.HttpWebRequest)System.Net.WebRequest.Create(url.ToString());
                        System.Net.HttpWebResponse HttpWResp = (System.Net.HttpWebResponse)HttpWReq.GetResponse(); // Insert code that uses the response object.
                        Stream receiveStream = HttpWResp.GetResponseStream();
                        System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
                        StreamReader readStream = new StreamReader(receiveStream, encode);

                        //Print("Response stream received");
                        string xmlData = readStream.ReadToEnd();
                        XmlTextReader reader = new XmlTextReader(new StringReader(xmlData));

                        while (reader.Read())
                        {
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == "status")
                            {
                                //Print(reader.ReadElementContentAsString());
                            }
                            if (reader.NodeType == XmlNodeType.Element && reader.Name == "elevation")
                            {
                                outPts.Add(reader.ReadElementContentAsDouble());
                            }
                        }


                        reader.Close();
                        HttpWResp.Close();
                        // Releases the resources of the Stream.
                        readStream.Close();

                        //allUrl.Add(url.ToString());

                        url.Remove(0, url.Length);
                        url.Append(preUrl);


                    }
                    catch (Exception ex)
                    {
                        sb = new System.Text.StringBuilder(ex.Message);
                        //Print("Exception: " + sb.ToString());
                    }

                }
                else
                {
                    url.Append("|");
                }

            }
           // A = allElev;
            DA.SetDataList(0, outPts);


        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{fd618634-b121-4237-86ef-9074040e2a59}"); }
        }

        protected override Bitmap Icon
        {
            get
            {
                return Resources.gHowl_elev;
            }
        }
    }
}
