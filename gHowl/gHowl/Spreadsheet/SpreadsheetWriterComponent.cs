using System;
using System.Collections.Generic;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Parameters;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;
using System.Drawing;
using gHowl.Properties;


namespace gHowl.Spreadheet
{
    public class SpreadsheetWriter : GH_Component, IGH_VarParamComponent
    {
        public SpreadsheetWriter()
            : base("Spreadsheet Writer", "#W", "Write GH Data to a Spreadsheet", "gHowl", "#")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_StringParam("Path", "P", "The path and filename for the spreadsheet you want to create", GH_ParamAccess.item);
            pManager[0].Optional = false;
            pManager.Register_GenericParam("Sheet Data 1", "Sheet 1", "Data to be written to Sheet #1", GH_ParamAccess.tree);
            pManager[1].Optional = false;

        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.Register_GenericParam("Info", "I", "Information Message");

        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {
            string path = null;
            GH_Structure<IGH_Goo> data = new GH_Structure<IGH_Goo>();
            if (!DA.GetData(0, ref path)) { return; }

            //if (!DA.GetDataTree(1, out data)) { return; }
            ClosedXML.Excel.XLWorkbook wb = new ClosedXML.Excel.XLWorkbook();
            int num = this.Params.Input.Count - 1;
            for (int p = 1; p <= num; p++)
            {
                DA.GetDataTree<IGH_Goo>(p, out data);
                string sheetName = this.Params.Input[p].NickName;
                var ws = wb.Worksheets.Add(sheetName, p);
                for (int i = 0; i < data.PathCount; i++)
                {
                    for (int j = 0; j < data.Branches[i].Count; j++)
                    {
                        ws.Cell(i + 1, j + 1).Value = data[data.Paths[i]][j];
                    }
                }

            }

            wb.SaveAs(path);
            DA.SetData(0,"Spreadsheet saved to: "+path);
        }
        protected override Bitmap Icon
        {
            get
            {
                return Resources.spreadSheetOut;
            }
        }
        public override Guid ComponentGuid
        {
            get { return new Guid("{70e5b445-4391-4609-a822-0834fc544fa3}"); }
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
            if (e.Index <= 1)
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
            string nname = "Sheet " + index;
            if (param.NickName != nname && param.NickName != "Data")
            {
                param.NickName = string.Format("{0}", param.NickName);
                param.Name = string.Format("Sheet Data {0}", param.NickName);
                param.Description = string.Format("Data to be written to the worksheet named: {0}", param.NickName);
            }
            else if (param.NickName == "Data")
            {               
                param.NickName = string.Format("{0}", nname);
                param.Name = string.Format("Sheet Data {0}", nname);
                param.Description = string.Format("Data to be written to the worksheet named: {0}", nname);
            }

            param.Access = GH_ParamAccess.tree;
            param.Optional = true;
        }

        public void ParametersModified(GH_VarParamSide side)
        {
            if (side == GH_VarParamSide.Input)
            {
                int num2 = this.Params.Input.Count - 1;
                for (int i = 1; i <= num2; i++)
                {
                    this.FixParameterFields(this.Params.Input[i], i);
                }
            }



        }



    }
}
