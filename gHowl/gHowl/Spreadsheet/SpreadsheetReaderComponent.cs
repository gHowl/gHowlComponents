using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel;
using System.Drawing;
using gHowl.Properties;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

namespace gHowl.Spreadheet
{
    public class SpreadsheetReaderComponent : SafeComponent
    {
        public SpreadsheetReaderComponent()
            : base("Spreadsheet Reader", "#R", "Import spreadsheet data to GH", "gHowl", "#")
        {
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.Register_StringParam("Path", "P", "Path to file",GH_ParamAccess.item);
            pManager[0].Optional = false;

            pManager.Register_IntegerParam("Sheet", "S", "Sheet to work with.  If no sheet number is supplied, default will be the first sheet in the document", 1,GH_ParamAccess.item);
            pManager[1].Optional = true;
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {

            pManager.Register_StringParam("Message", "M", "Service Messages");
            pManager.Register_StringParam("Data", "D", "Data from the Spreadsheet Document");

        }


        protected override void SolveInstance(IGH_DataAccess DA, bool secondOrLaterRun)
        {
           
                //DA.SetData(0, "OpenOffice was found on this system");

                string path = null;
                if (!DA.GetData(0, ref path)) return;
                int sheet = 0;
                if (!DA.GetData(1, ref sheet)) return;
                GH_Structure<GH_String> data = new GH_Structure<GH_String>();
                ClosedXML.Excel.XLWorkbook workbook = new ClosedXML.Excel.XLWorkbook(path);
                var ws = workbook.Worksheet(sheet);

                for (int i = 1; i <= ws.LastCellUsed().Address.RowNumber; i++)
                {
                    for (int j = 1; j <= ws.LastCellUsed().Address.ColumnNumber; j++)
                    {
                        GH_Path p = new GH_Path(i-1);
                        GH_String cellData = new GH_String();      
                        cellData.Value = ws.Cell(i, j).Value.ToString();

                        data.Append(cellData, p);
   
                    }
                
                }
            
               // DA.SetData(0, ws.LastCellUsed().Address.RowNumber.ToString());
               // DA.SetData(1, ws.LastCellUsed().Address.ColumnNumber);
                DA.SetDataTree(1, data);
            
        }

        
        //Utility Methods
        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{73cb7ac5-de4f-486e-8243-e797fbcb45b0}");
            }
        }
        protected override Bitmap Icon
        {
            get
            {
                return Resources.spreadSheetIn;
            }
        }

    }
}
