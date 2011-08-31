﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;

namespace ClosedXML.Excel
{
    internal class XLWorksheets : IXLWorksheets,IEnumerable<XLWorksheet>
    {
        #region Constructor
        private readonly Dictionary<String, XLWorksheet> m_worksheets = new Dictionary<String, XLWorksheet>();
        private readonly XLWorkbook m_workbook;
        #endregion
        public HashSet<String> Deleted = new HashSet<String>();
        #region Constructor
        public XLWorksheets(XLWorkbook workbook)
        {
            m_workbook = workbook;
        }
        #endregion
        #region IXLWorksheets Members
        public int Count
        {
            [DebuggerStepThrough]
            get { return m_worksheets.Count; }
        }

        public bool TryGetWorksheet(string sheetName, out IXLWorksheet worksheet)
        {
            XLWorksheet w;
            if (m_worksheets.TryGetValue(sheetName, out w))
            {
                worksheet = w;
                return true;
            }
            worksheet = null;
            return false;
        }

        public IXLWorksheet Worksheet(String sheetName)
        {
            return m_worksheets[sheetName];
        }

        public IXLWorksheet Worksheet(Int32 position)
        {
            var wsCount = m_worksheets.Values.Where(w => w.Position == position).Count();
            if (wsCount == 0)
            {
                throw new Exception("There isn't a worksheet associated with that position.");
            }

            if (wsCount > 1)
            {
                throw new Exception("Can't retrieve a worksheet because there are multiple worksheets associated with that position.");
            }

            return m_worksheets.Values.Where(w => w.Position == position).Single();
        }

        public void Rename(String oldSheetName, String newSheetName)
        {
            if (!StringExtensions.IsNullOrWhiteSpace(oldSheetName) && m_worksheets.ContainsKey(oldSheetName))
            {
                var ws = m_worksheets[oldSheetName];
                m_worksheets.Remove(oldSheetName);
                m_worksheets.Add(newSheetName, ws);
            }
        }

        public IXLWorksheet Add(String sheetName)
        {
            var sheet = new XLWorksheet(sheetName, m_workbook);
            m_worksheets.Add(sheetName, sheet);
            sheet._position = m_worksheets.Count;
            return sheet;
        }

        public IXLWorksheet Add(String sheetName, Int32 position)
        {
            var ws = Add(sheetName);
            ws.Position = position;
            return ws;
        }

        public void Delete(String sheetName)
        {
            Delete(m_worksheets[sheetName].Position);
        }

        public void Delete(Int32 position)
        {
            var wsCount = m_worksheets.Values.Where(w => w.Position == position).Count();
            if (wsCount == 0)
            {
                throw new Exception("There isn't a worksheet associated with that index.");
            }

            if (wsCount > 1)
            {
                throw new Exception("Can't delete the worksheet because there are multiple worksheets associated with that index.");
            }

            var ws = m_worksheets.Values.Where(w => w.Position == position).Single();
            if (!StringExtensions.IsNullOrWhiteSpace(ws.RelId) && !Deleted.Contains(ws.RelId))
            {
                Deleted.Add(ws.RelId);
            }

            m_worksheets.RemoveAll(w => w.Position == position);
            m_worksheets.Values.Where(w => w.Position > position).ForEach(w => (w)._position -= 1);
        }
        #endregion
        #region IEnumerable<IXLWorksheet> Members
        public IEnumerator<XLWorksheet> GetEnumerator()
        {
            foreach (var w in m_worksheets.Values)
            {
                yield return w;
            }
        }
        #endregion
        #region IEnumerable<XLWorksheet> Members
        IEnumerator<IXLWorksheet> IEnumerable<IXLWorksheet>.GetEnumerator()
        {
            foreach (var w in m_worksheets.Values)
            {
                yield return w;
            }
        }
        #endregion
        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
        public IXLWorksheet Add(DataTable dataTable)
        {
            var ws = Add(dataTable.TableName);
            ws.Cell(1, 1).InsertTable(dataTable.AsEnumerable());
            ws.Columns().AdjustToContents(1, 75);
            return ws;
        }
        public void Add(DataSet dataSet)
        {
            foreach (DataTable t in dataSet.Tables)
            {
                Add(t);
            }
        }
    }
}