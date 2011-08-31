﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ClosedXML.Excel
{
    using System.Collections;

    internal class XLRanges : IXLRanges, IXLStylized
    {
        public Boolean StyleChanged { get; set; }
        private readonly List<XLRange> _ranges = new List<XLRange>();
        private Int32 _count;
        private IXLStyle _style;

        public XLRanges()
        {
            _style = new XLStyle(this, XLWorkbook.DefaultStyle);
        }

        #region IXLRanges Members

        public IXLRanges Clear(XLClearOptions clearOptions = XLClearOptions.ContentsAndFormats)
        {
            _ranges.ForEach(c => c.Clear(clearOptions));
            return this;
        }

        public void Add(IXLRangeBase range)
        {
            _count++;
            _ranges.Add(range.AsRange() as XLRange);
        }

        public void Add(IXLCell cell)
        {
            Add(cell.AsRange());
        }

        //public void Add(String rangeAddress)
        //{
        //    count++;
        //    var byExclamation = rangeAddress.Split('!');
        //    var wsName = byExclamation[0].Replace("'", "");
        //    var rng = byExclamation[1];
        //    var rangeToAdd = workbook.Worksheets.Worksheet(wsName).Range(rng);
        //    ranges.Add((XLRange)rangeToAdd);
        //}
        public void Remove(IXLRange range)
        {
            _count--;
            _ranges.RemoveAll(r => r.ToString() == range.ToString());
        }

        public Int32 Count
        {
            get { return _count; }
        }

        public IEnumerator<IXLRange> GetEnumerator()
        {
            var retList = new List<IXLRange>();
            _ranges.ForEach(retList.Add);
            return retList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public IXLStyle Style
        {
            get { return _style; }
            set
            {
                _style = new XLStyle(this, value);
                foreach (XLRange rng in _ranges)
                    rng.Style = value;
            }
        }

        public Boolean Contains(IXLRange range)
        {
            return _ranges.Any(r => !r.RangeAddress.IsInvalid && r.Equals(range));
        }

        public IXLDataValidation DataValidation
        {
            get
            {
                foreach (XLRange range in _ranges)
                {
                    foreach (IXLDataValidation dv in range.Worksheet.DataValidations)
                    {
                        foreach (IXLRange dvRange in dv.Ranges.Where(dvRange => dvRange.Intersects(range)))
                        {
                            dv.Ranges.Remove(dvRange);
                            foreach (IXLCell c in dvRange.Cells().Where(c => !range.Contains(c.Address.ToString())))
                            {
                                dv.Ranges.Add(c.AsRange());
                            }
                        }
                    }
                }
                var dataValidation = new XLDataValidation(this);

                _ranges.First().Worksheet.DataValidations.Add(dataValidation);
                return dataValidation;
            }
        }

        public IXLRanges AddToNamed(String rangeName)
        {
            return AddToNamed(rangeName, XLScope.Workbook);
        }

        public IXLRanges AddToNamed(String rangeName, XLScope scope)
        {
            return AddToNamed(rangeName, XLScope.Workbook, null);
        }

        public IXLRanges AddToNamed(String rangeName, XLScope scope, String comment)
        {
            _ranges.ForEach(r => r.AddToNamed(rangeName, scope, comment));
            return this;
        }

        public Object Value
        {
            set { _ranges.ForEach(r => r.Value = value); }
        }

        public IXLRanges SetValue<T>(T value)
        {
            _ranges.ForEach(r => r.SetValue(value));
            return this;
        }

        public IXLCells Cells()
        {
            var cells = new XLCells(false, false);
            foreach (XLRange container in _ranges)
                cells.Add(container.RangeAddress);
            return cells;
        }

        public IXLCells CellsUsed()
        {
            var cells = new XLCells(true, false);
            foreach (XLRange container in _ranges)
                cells.Add(container.RangeAddress);
            return cells;
        }

        public IXLCells CellsUsed(Boolean includeStyles)
        {
            var cells = new XLCells(true, includeStyles);
            foreach (XLRange container in _ranges)
                cells.Add(container.RangeAddress);
            return cells;
        }

        public IXLRanges SetDataType(XLCellValues dataType)
        {
            _ranges.ForEach(c => c.DataType = dataType);
            return this;
        }

        #endregion

        #region IXLStylized Members

        public IEnumerable<IXLStyle> Styles
        {
            get
            {
                UpdatingStyle = true;
                yield return _style;
                foreach (XLRange rng in _ranges)
                {
                    yield return rng.Style;
                    foreach (XLCell r in rng.Worksheet.Internals.CellsCollection.GetCells(
                        rng.RangeAddress.FirstAddress.RowNumber,
                        rng.RangeAddress.FirstAddress.ColumnNumber,
                        rng.RangeAddress.LastAddress.RowNumber,
                        rng.RangeAddress.LastAddress.ColumnNumber))
                        yield return r.Style;
                }
                UpdatingStyle = false;
            }
        }

        public Boolean UpdatingStyle { get; set; }

        public IXLStyle InnerStyle
        {
            get { return _style; }
            set { _style = new XLStyle(this, value); }
        }

        public IXLRanges RangesUsed
        {
            get { return this; }
        }

        #endregion

        public override string ToString()
        {
            String retVal = _ranges.Aggregate(String.Empty, (agg, r) => agg + (r.ToString() + ","));
            if (retVal.Length > 0) retVal = retVal.Substring(0, retVal.Length - 1);
            return retVal;
        }

        public override bool Equals(object obj)
        {
            var other = (XLRanges)obj;

            return _ranges.Count == other._ranges.Count && _ranges.Select(thisRange => Enumerable.Contains(other._ranges, thisRange)).All(foundOne => foundOne);
        }

        public override int GetHashCode()
        {
            return _ranges.Aggregate(0, (current, r) => current ^ r.GetHashCode());
        }
    }
}