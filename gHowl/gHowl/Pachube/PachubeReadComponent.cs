using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;

namespace gHowl.Pachube
{
    public class PachubeReadComponent : SafeComponent
    {
        private string _serviceMessage = null;

        public PachubeReadComponent() : base("Read Pachube", "Pachube", "This component reads a Pachube Feed", "gHowl", "XML") { }

        protected override void RegisterInputParams(Grasshopper.Kernel.GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_StringParam("Api Key", "A", "Your Pachube API Key", GH_ParamAccess.item);
            pManager[0].Optional = false;

            pManager.Register_StringParam("Feed URL", "U", "The Pachube feed to grab","http://www.pachube.com/api/feeds/1197.xml" ,GH_ParamAccess.item);
            pManager[1].Optional = true;

            pManager.Register_StringParam("Path", "P", "If you want to save a feed to an xml file, input the name of the file here",GH_ParamAccess.item);
            pManager[2].Optional = true;
        }

        protected override void RegisterOutputParams(Grasshopper.Kernel.GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_StringParam("Info", "I", "Network information");
            pManager.Register_StringParam("Data", "D", "The string which was just received");
        }

        protected override void SolveInstance(Grasshopper.Kernel.IGH_DataAccess DA, bool secondOrLaterRun)
        {
            String apikey=null;
            String feedURL =null;
            String path=null;
            if (!DA.GetData(0, ref apikey)) { }
            if (!DA.GetData(1, ref feedURL)) { }
            if (!DA.GetData(2, ref path)) { }
            System.Net.WebClient wc = new System.Net.WebClient();
            wc.Headers.Add("X-PachubeApiKey", apikey);
            byte[] dataIn = wc.DownloadData(feedURL);

            //If a path is supplied, also download the data to a file
            if (path != null)
            {
                wc.DownloadFile(feedURL, path);
            }
            

            wc.Dispose();

            System.IO.MemoryStream memoryStream = new System.IO.MemoryStream(dataIn);
            memoryStream.Position = 0;

            System.Xml.XmlDocument xmlDoc = new System.Xml.XmlDocument();
            xmlDoc.Load(memoryStream);



           

            // create some XML nodes to get the data out of the xml
            System.Xml.XmlNodeList title = xmlDoc.GetElementsByTagName("title");
            System.Xml.XmlNodeList data = xmlDoc.GetElementsByTagName("data");
           
            _serviceMessage = ("Connected To: " + title[0].ChildNodes.Item(0).InnerText + "\n" + System.DateTime.Now.ToString() + "\n" + data.Count.ToString() + " Data Elements");
          
            List<string> values = new List<string>();
            for (Int32 i = 0; i < data.Count; i++)
            {
                for (Int32 j = 0; j < data[i].ChildNodes.Count; j++)
                {
                    values.Add(data[i].ChildNodes.Item(j).InnerText);

                }

            }
            DA.SetData(0, _serviceMessage);
            DA.SetDataList(1, values);
            //A = values;
        }
        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                return gHowl.Properties.Resources.pachubeIn;
            }
        }
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{f3054896-b62a-4fa8-b009-d43511234084}");
            }

        }   
    }
}
