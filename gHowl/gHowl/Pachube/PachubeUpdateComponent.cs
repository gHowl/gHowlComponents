using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;
using System.Xml;
using Grasshopper.Kernel.Parameters;
using System.Drawing;
using gHowl.Properties;

namespace gHowl.Pachube
{
    public class PachubeUpdateComponent : GH_Component, IGH_VarParamComponent
    {
        public PachubeUpdateComponent() : base("Pachube Update", "Update Pachube", "Updates a Pachube Feed", "gHowl", "XML") { }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            
            pManager.Register_StringParam("API Key", "A", "Pachube API key.  You must have a Pachube account in order to obtain your API key.", GH_ParamAccess.item);
            pManager.Register_StringParam("Url", "@", "Pachube Feed Url.  Example: http://api.pachube.com/v2/feeds/12345.xml. \n Should have .xml or .csv as the suffix.", GH_ParamAccess.item);
           // pManager.Register_IntegerParam("Datastream ID(s)", "I", "The ID of the datastream you want to update. Can input multiple IDs at a time. \n NOTE: The number of IDs bust match the number of data items", GH_ParamAccess.list);
            pManager.Register_GenericParam("Datastream ", "0", "Datastream ID = 0", GH_ParamAccess.item);
            pManager[0].Optional = false;
            pManager[1].Optional = false;
            pManager[2].Optional = false;
            //pManager[3].Optional = false;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
           
            pManager.Register_StringParam("Data", "d", "Data Sent");

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string url = "";
            string api = "";
            List<int> id = new List<int>();
            object data = null;
            string msg = "";

            if (!DA.GetData(1, ref url) ||
                !DA.GetData(0, ref api)
                //!DA.GetData<object>(2, ref data)
                ) { return; }
          /*  if (id.Count != data.Count)
            {
                msg = "Datastream ID and Data Values must match in quantity.";
                DA.SetData(0, msg);
                return;
            }
            else
            {*/
                System.Net.WebClient wc = new System.Net.WebClient();

                wc.Headers.Add("X-PachubeApiKey", api);

                System.Text.StringBuilder sb = new System.Text.StringBuilder();


                if (url.Contains(".xml"))
                {
                    XmlWriterSettings settings = new XmlWriterSettings();

                    settings.Indent = true;
                    settings.OmitXmlDeclaration = true;

                    using (XmlWriter writer = XmlWriter.Create(sb, settings))
                    {

                        writer.WriteStartDocument();
                        writer.WriteStartElement("eeml", "http://www.eeml.org/xsd/0.5.1");
                        writer.WriteAttributeString("xmlns", "", null, "http://www.eeml.org/xsd/0.5.1");
                        writer.WriteAttributeString("xmlns", "xsi", null, "http://www.w3.org/2001/XMLSchema-instance");
                        writer.WriteAttributeString("version", "0.5.1");
                        writer.WriteAttributeString("xsi", "schemaLocation", null, "http://www.eeml.org/xsd/0.5.1 http://www.eeml.org/xsd/0.5.1/0.5.1.xsd");
                        writer.WriteStartElement("environment");
                        int num = this.Params.Input.Count;
                        for (int i = 2; i < num; i++)
                        {
                            DA.GetData(i, ref data);
                            if (data != null)
                            {
                                writer.WriteStartElement("data");
                                writer.WriteAttributeString("id", this.Params.Input[i].NickName.ToString());
                                writer.WriteStartElement("current_value");
                                writer.WriteString(data.ToString());
                                writer.WriteEndElement();
                                writer.WriteEndElement();
                            }
                        }

                        writer.WriteEndElement();
                        writer.WriteEndElement();
                        writer.WriteEndDocument();
                    }
                }
                else if (url.Contains(".csv"))
                {
                    int num = this.Params.Input.Count;
                    for (int i = 2; i < num; i++)
                    {
                        DA.GetData(i, ref data);
                        if (data != null)
                        {
                            sb.Append(this.Params.Input[i].NickName.ToString() + "," + data.ToString() + "\n");
                            data = null;
                        }
                    }
                }
                else 
                {
                    msg = "Feed url incorrectly formatted, must have .xml or .csv suffix";
                    DA.SetData(0, msg);
                    return;
                }

                byte[] d = System.Text.Encoding.UTF8.GetBytes(sb.ToString());
                wc.UploadData(url, "PUT", d);
                wc.Dispose();
                DA.SetData(0, sb.ToString());
            

           
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("{12f1943c-7f49-461c-bf1f-ececba590ba2}"); }
        }

        protected override Bitmap Icon
        {
            get
            {
                return Resources.pachubeOut;
            }
        }

        public IGH_Param ConstructVariable(GH_VarParamEventArgs e)
        {
            Param_GenericObject param = new Param_GenericObject();
            this.FixParameterFields(param, e.Index);
            return param;
        }

        public bool IsInputVariable
        {
            get { return true; }
        }

        public bool IsOutputVariable
        {
            get { return false; }
        }

        public bool IsVariableParam(GH_VarParamEventArgs e)
        {
            if (e.Side == GH_VarParamSide.Output)
            {
                return false;
            }
            if (e.Index <= 2)
            {
                return false;
            }
            return true;
        }

        public void ManagerConstructed(GH_VarParamSide side, Grasshopper.GUI.GH_VariableParameterManager manager)
        {
            
        }
        private void FixParameterFields(IGH_Param param, int index)
        {
            
            if (param.NickName != (index-2).ToString() && param.NickName != "Data")
            {
                param.NickName = string.Format("{0}", param.NickName);
                param.Name = string.Format("Datastream {0}", param.NickName);
                param.Description = string.Format("Datastream ID = {0}", param.NickName);
            }
            else if (param.NickName == "Data" || param.NickName == (index-2).ToString())
            {
                param.NickName = string.Format("{0}", index-2);
                param.Name = string.Format("Datastream {0}", index-2);
                param.Description = string.Format("Datastream ID = {0}", index-2);
            }
           
            param.Access = GH_ParamAccess.item;
            param.Optional = true;
            
        }

        public void ParametersModified(GH_VarParamSide side)
        {
            if (side == GH_VarParamSide.Input)
            {
                int cnt = 0;
                int num2 = this.Params.Input.Count-1;
                for (int i = 2; i <= num2; i++)
                {
                   
                        this.FixParameterFields(this.Params.Input[i],cnt);
                  
                    cnt++;
                }
            }
        }
    }
}
