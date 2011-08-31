using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Net;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using gHowl.Properties;

namespace gHowl.Xml {
	enum XmlInputType { Unknown, File, WebAddress, XmlString }

	public class ParseXml : GH_Component {

		public override Guid ComponentGuid {
			get { return new Guid("1987c033-10c3-4259-85e2-8da173856eb4"); }
		}

		public ParseXml() : base("Xml Parser", "XML", "Parses an XML File", "gHowl", "XML") { }

		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
			pManager.Register_StringParam("Path", "P", "Path of the Xml File to Parse");
		}

        protected override System.Drawing.Bitmap Icon
        {
            get { return Resources.xmlParse; }
        }

		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
			pManager.Register_StringParam("Elements", "E", "Xml Elements");
			pManager.Register_StringParam("Values", "V", "Element Values");
			pManager.Register_StringParam("Attribute Names", "A", "Attribute Names");
			pManager.Register_StringParam("Attribute Values", "V", "Attribute Values");
			pManager.Register_StringParam("CData", "CD", "CData");
		}
		
		protected override void SolveInstance(IGH_DataAccess DA) {
			string XmlPath = null;
			XmlInputType InputType = XmlInputType.Unknown;

			DA.GetData(0, ref XmlPath);
			if (XmlPath == null) {
				this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Xml Path is Null.");
			}

			XmlTextReader XmlRdr = null;

			if (File.Exists(XmlPath)) {
				InputType = XmlInputType.File;
			}
			else if (XmlPath.Contains("http://")) {
				InputType = XmlInputType.WebAddress;
			}
			else if (XmlPath.Contains("<")) {
				InputType = XmlInputType.XmlString;
			}

			switch (InputType) {
				case XmlInputType.File:
					FileStream fStream = new FileStream(XmlPath, FileMode.Open);
					if (fStream == null) {
						this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "File Stream is Null");
					}

					XmlRdr = new XmlTextReader(fStream);
					if (XmlRdr == null) {
						this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Xml Reader is Null");
					}
					break;

				case XmlInputType.WebAddress:
					WebRequest WbRqst = WebRequest.Create(XmlPath);
					WebResponse WbRspn;

					//Header for Pachube authentication...
					//WbRqst.Headers.Add("X-PachubeApiKey", API_KEY_VAR);

					try {
						WbRspn = WbRqst.GetResponse();
						XmlRdr = new XmlTextReader(WbRspn.GetResponseStream());
					}
					catch {
						this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Web Request could not be successfully retrieved");
						InputType = XmlInputType.Unknown;
					}
					break;

				case XmlInputType.XmlString:
					MemoryStream mStream = new MemoryStream(Encoding.UTF8.GetBytes(XmlPath));
					XmlRdr = new XmlTextReader(mStream);
					break;

				default:
					this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Input Type Unknown");
					return;
			}

			//Out Lists...
			DataTree<string> ElementTree = new DataTree<string>();
			DataTree<string> ValueTree = new DataTree<string>();
			DataTree<string> AttrNameTree = new DataTree<string>();
			DataTree<string> AttrValTree = new DataTree<string>();
			DataTree<string> CDataTree = new DataTree<string>();

			List<int> NodeList = new List<int>();
			NodeList.Add(0);
			XmlNodeType LastNodeType = XmlNodeType.None;

			//Loop through Xml reader...
			while (XmlRdr.Read()) {
				//manage the tree...
				if ((LastNodeType == XmlNodeType.Element) & (XmlRdr.NodeType == XmlNodeType.Element)) {   //Add another node to the list..
					NodeList.Add(0);
				}

				if ((LastNodeType == XmlNodeType.EndElement) & (XmlRdr.NodeType == XmlNodeType.Element)) {   //Increment last node in the list...
					NodeList[NodeList.Count - 1]++;
				}

				if ((LastNodeType == XmlNodeType.EndElement) & (XmlRdr.NodeType == XmlNodeType.EndElement)) {   //Remove Last Node in List...
					if (NodeList.Count > 0) {
						NodeList.RemoveAt(NodeList.Count - 1);
					}
				}

				//handle the different nodes...
				switch (XmlRdr.NodeType) {
					case XmlNodeType.Attribute:
						break;
					case XmlNodeType.CDATA:
						CDataTree.Add(XmlRdr.Value, new GH_Path(NodeList.ToArray()));
						break;
					case XmlNodeType.Element:
						ElementTree.Add(XmlRdr.Name, new GH_Path(NodeList.ToArray()));

						if (XmlRdr.AttributeCount > 0) {
							for (int i = 1 ; i <= XmlRdr.AttributeCount ; i++) {
								XmlRdr.MoveToAttribute(i - 1);
								AttrNameTree.Add(XmlRdr.Name, new GH_Path(NodeList.ToArray()));
								AttrValTree.Add(XmlRdr.Value, new GH_Path(NodeList.ToArray()));
							}
						}

						if (XmlRdr.IsEmptyElement) {
							LastNodeType = XmlNodeType.EndElement;
						}
						else {
							LastNodeType = XmlNodeType.Element;
						}
						break;
					case XmlNodeType.EndElement:
						LastNodeType = XmlRdr.NodeType;
						break;
					case XmlNodeType.Entity:
						this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Entities Not Supported");
						break;
					case XmlNodeType.EntityReference:
						this.AddRuntimeMessage(GH_RuntimeMessageLevel.Warning, "Entity References Not Supported");
						break;
					case XmlNodeType.Text:
						ValueTree.Add(XmlRdr.Value, new GH_Path(NodeList.ToArray()));
						break;
					#region Unsupported Xml Node Types
					//case XmlNodeType.Comment:
					//    break;
					//case XmlNodeType.Document:
					//    break;
					//case XmlNodeType.DocumentFragment:
					//    break;
					//case XmlNodeType.DocumentType:
					//    break;
					//case XmlNodeType.EndEntity:
					//    break;
					//case XmlNodeType.None:
					//    break;
					//case XmlNodeType.Notation:
					//    break;
					//case XmlNodeType.ProcessingInstruction:
					//    break;
					//case XmlNodeType.SignificantWhitespace:
					//    break;
					//case XmlNodeType.Whitespace:
					//    break;
					//case XmlNodeType.XmlDeclaration:
					//    break; 
					#endregion
					default:
						break;
				}
			}

			DA.SetDataTree(0, ElementTree);
			DA.SetDataTree(1, ValueTree);
			DA.SetDataTree(2, AttrNameTree);
			DA.SetDataTree(3, AttrValTree);
			DA.SetDataTree(4, CDataTree);
		}
	}

}
