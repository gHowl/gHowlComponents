using System;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Text;

using Grasshopper;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace gHowl.Xml {
	public class XmlWriter
		: GH_Component {

		public override Guid ComponentGuid {
			get { return new Guid(@"d2c5be07-759c-482c-a488-a5ded179989e"); }
		}

		public XmlWriter()
			: base("Write XML", "XML", "Writes a data in a Grasshopper Tree to an XML file", "gHowl", "XML") { }

		protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager) {
			pManager.Register_GenericParam("Data", "D", "Generic Data that will be written to XML.", GH_ParamAccess.tree);
			pManager.Register_StringParam("Element Names", "E", "The element names that will be used for the data in the XML.", GH_ParamAccess.tree);
			pManager[1].Optional = true;
		}

		protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager) {
			pManager.Register_GenericParam("XML", "X", "A string representing the XML");
		}

		protected override System.Drawing.Bitmap Icon {
			get { return gHowl.Properties.Resources.xmlWrite; }
		}

		protected override void SolveInstance(IGH_DataAccess DA) {

			GH_Structure<IGH_Goo> genericData = new GH_Structure<IGH_Goo>();
			if ( !DA.GetDataTree<IGH_Goo>(0, out genericData) ) {
				this.AddRuntimeMessage(GH_RuntimeMessageLevel.Error, "Generic Data could not be retrieved.");
			}

			GH_Structure<GH_String> suppliedElements = new GH_Structure<GH_String>();
			DA.GetDataTree<GH_String>(1, out suppliedElements);

			GH_Path startPath = new GH_Path(0);
			XElement xml = WritePath(null, startPath, genericData, suppliedElements, false);

			DA.SetData(0, xml.ToString());
		}

		private XElement WritePath(XElement previousElement, GH_Path path, GH_Structure<IGH_Goo> data, GH_Structure<GH_String> elementNames, bool calledBySibling) {
			XElement currentElement = null;
			string name = elementNames[path] == null ? PathToXmlElementName(path) : elementNames[path][0].ToString();

			#region Write current data

			if ( previousElement == null ) {
				if ( data[path] == null ) {
					currentElement = new XElement(name);
				}
				else {
					currentElement = new XElement(name, data[path][0].ToString());
					if ( data[path].Count > 1 ) {
						this.AddRuntimeMessage(
							GH_RuntimeMessageLevel.Warning,
							string.Format("{0} contains multiple data elements. Only the first data element will be written.", path)
							);
					}
				}
				previousElement = currentElement;
			}
			else {
				if ( data[path] == null ) {
					currentElement = new XElement(name);
				}
				else {
					currentElement = new XElement(name, data[path][0].ToString());
					if ( data[path].Count > 1 ) {
						this.AddRuntimeMessage(
							GH_RuntimeMessageLevel.Warning,
							string.Format("{0} contains multiple data elements. Only the first data element will be written.", path)
						);
					}
				}
				previousElement.Add(currentElement);
			}

			#endregion

			#region Write Descendents

			if ( path.HasDescendents(data.Paths) ) {
				WritePath(currentElement, path.AppendElement(0), data, elementNames, false);
			}
			
			#endregion

			#region Write Siblings

			if ( !calledBySibling ) {
				List<GH_Path> siblings = new List<GH_Path>();
				siblings = (List<GH_Path>)path.FindApparentSiblings(data.Paths);
				if ( siblings != null ) {
					foreach ( GH_Path sibling in siblings ) {
						WritePath(previousElement, sibling, data, elementNames, true);
					}
				}
			}

			#endregion

			return previousElement;
		}

		private string PathToXmlElementName(GH_Path path) {
			StringBuilder nameBuilder = new StringBuilder("GHPath_");
			for ( int i = 0 ; i < path.Length ; i++ ) {
				nameBuilder.Append(path[i]);
				if ( i != path.Length - 1 ) {
					nameBuilder.Append('-');
				}
			}
			return nameBuilder.ToString();
		}

	}
}
