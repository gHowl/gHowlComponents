﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ClosedXML.Excel
{
    using System.Linq;

    public enum XLCalculateMode
    {
        Auto,
        AutoNoTable,
        Manual,
        Default
    };

    public enum XLReferenceStyle
    {
        R1C1,
        A1,
        Default
    };

    public enum XLCellSetValueBehavior
    {
        /// <summary>
        ///   Analyze input string and convert value. For avoid analyzing use escape symbol '
        /// </summary>
        Smart = 0,

        /// <summary>
        ///   Direct set value. If value has unsupported type - value will be stored as string returned by <see
        ///    cref = "object.ToString()" />
        /// </summary>
        Simple = 1,
    }

    public partial class XLWorkbook
    {
        #region Static

        private static IXLStyle _defaultStyle;

        public static IXLStyle DefaultStyle
        {
            get
            {
                return _defaultStyle ?? (_defaultStyle = new XLStyle(null)
                                                             {
                                                                 Font = new XLFont(null, null)
                                                                            {
                                                                                Bold = false,
                                                                                Italic = false,
                                                                                Underline = XLFontUnderlineValues.None,
                                                                                Strikethrough = false,
                                                                                VerticalAlignment =
                                                                                    XLFontVerticalTextAlignmentValues.
                                                                                    Baseline,
                                                                                FontSize = 11,
                                                                                FontColor = XLColor.FromArgb(0, 0, 0),
                                                                                FontName = "Calibri",
                                                                                FontFamilyNumbering =
                                                                                    XLFontFamilyNumberingValues.Swiss
                                                                            },
                                                                 Fill = new XLFill(null)
                                                                            {
                                                                                BackgroundColor = XLColor.FromIndex(64),
                                                                                PatternType = XLFillPatternValues.None,
                                                                                PatternColor = XLColor.FromIndex(64)
                                                                            },
                                                                 Border = new XLBorder(null, null)
                                                                              {
                                                                                  BottomBorder =
                                                                                      XLBorderStyleValues.None,
                                                                                  DiagonalBorder =
                                                                                      XLBorderStyleValues.None,
                                                                                  DiagonalDown = false,
                                                                                  DiagonalUp = false,
                                                                                  LeftBorder = XLBorderStyleValues.None,
                                                                                  RightBorder = XLBorderStyleValues.None,
                                                                                  TopBorder = XLBorderStyleValues.None,
                                                                                  BottomBorderColor = XLColor.Black,
                                                                                  DiagonalBorderColor = XLColor.Black,
                                                                                  LeftBorderColor = XLColor.Black,
                                                                                  RightBorderColor = XLColor.Black,
                                                                                  TopBorderColor = XLColor.Black
                                                                              },
                                                                 NumberFormat =
                                                                     new XLNumberFormat(null, null) {NumberFormatId = 0},
                                                                 Alignment = new XLAlignment(null)
                                                                                 {
                                                                                     Indent = 0,
                                                                                     Horizontal =
                                                                                         XLAlignmentHorizontalValues.
                                                                                         General,
                                                                                     JustifyLastLine = false,
                                                                                     ReadingOrder =
                                                                                         XLAlignmentReadingOrderValues.
                                                                                         ContextDependent,
                                                                                     RelativeIndent = 0,
                                                                                     ShrinkToFit = false,
                                                                                     TextRotation = 0,
                                                                                     Vertical =
                                                                                         XLAlignmentVerticalValues.
                                                                                         Bottom,
                                                                                     WrapText = false
                                                                                 },
                                                                 Protection = new XLProtection(null)
                                                                                  {
                                                                                      Locked = true,
                                                                                      Hidden = false
                                                                                  }
                                                             });
            }
        }

        public static Double DefaultRowHeight { get; private set; }
        public static Double DefaultColumnWidth { get; private set; }

        public static IXLPageSetup DefaultPageOptions
        {
            get
            {
                var defaultPageOptions = new XLPageSetup(null, null)
                                             {
                                                 PageOrientation = XLPageOrientation.Default,
                                                 Scale = 100,
                                                 PaperSize = XLPaperSize.LetterPaper,
                                                 Margins = new XLMargins
                                                               {
                                                                   Top = 0.75,
                                                                   Bottom = 0.5,
                                                                   Left = 0.75,
                                                                   Right = 0.75,
                                                                   Header = 0.5,
                                                                   Footer = 0.75
                                                               },
                                                 ScaleHFWithDocument = true,
                                                 AlignHFWithMargins = true,
                                                 PrintErrorValue = XLPrintErrorValues.Displayed,
                                                 ShowComments = XLShowCommentsValues.None
                                             };
                return defaultPageOptions;
            }
        }

        public static IXLOutline DefaultOutline
        {
            get
            {
                return new XLOutline(null)
                           {
                               SummaryHLocation = XLOutlineSummaryHLocation.Right,
                               SummaryVLocation = XLOutlineSummaryVLocation.Bottom
                           };
            }
        }

        /// <summary>
        ///   Behavior for <see cref = "IXLCell.set_Value" />
        /// </summary>
        public static XLCellSetValueBehavior CellSetValueBehavior { get; set; }

        #endregion

        private readonly Dictionary<Int32, UnsupportedSheet> _unsupportedSheets =
            new Dictionary<int, UnsupportedSheet>();

        private readonly Dictionary<Int32, IXLStyle> _stylesById = new Dictionary<int, IXLStyle>();
        private readonly Dictionary<IXLStyle, Int32> _stylesByStyle = new Dictionary<IXLStyle, Int32>();
       
        internal Int32 GetStyleId(IXLStyle style)
        {
            Int32 cached;
            if (_stylesByStyle.TryGetValue(style, out cached))
                return cached;

            var count = _stylesByStyle.Count;
            var styleToUse = new XLStyle(null, style);
            _stylesByStyle.Add(styleToUse, count);
            _stylesById.Add(count, styleToUse);
            return count;
        }

        internal IXLStyle GetStyleById(Int32 id)
        {
            return _stylesById[id];
        }

        #region  Nested Type: XLLoadSource

        private enum XLLoadSource
        {
            New,
            File,
            Stream
        };

        #endregion

        internal XLWorksheets WorksheetsInternal { get; private set; }

        /// <summary>
        ///   Gets an object to manipulate the worksheets.
        /// </summary>
        public IXLWorksheets Worksheets
        {
            get { return WorksheetsInternal; }
        }

        /// <summary>
        ///   Gets an object to manipulate this workbook's named ranges.
        /// </summary>
        public IXLNamedRanges NamedRanges { get; private set; }

        /// <summary>
        ///   Gets an object to manipulate this workbook's theme.
        /// </summary>
        public IXLTheme Theme { get; private set; }

        /// <summary>
        ///   Gets or sets the default style for the workbook.
        ///   <para>All new worksheets will use this style.</para>
        /// </summary>
        public IXLStyle Style { get; set; }

        /// <summary>
        ///   Gets or sets the default row height for the workbook.
        ///   <para>All new worksheets will use this row height.</para>
        /// </summary>
        public Double RowHeight { get; set; }

        /// <summary>
        ///   Gets or sets the default column width for the workbook.
        ///   <para>All new worksheets will use this column width.</para>
        /// </summary>
        public Double ColumnWidth { get; set; }

        /// <summary>
        ///   Gets or sets the default page options for the workbook.
        ///   <para>All new worksheets will use these page options.</para>
        /// </summary>
        public IXLPageSetup PageOptions { get; set; }

        /// <summary>
        ///   Gets or sets the default outline options for the workbook.
        ///   <para>All new worksheets will use these outline options.</para>
        /// </summary>
        public IXLOutline Outline { get; set; }

        /// <summary>
        ///   Gets or sets the workbook's properties.
        /// </summary>
        public XLWorkbookProperties Properties { get; set; }

        /// <summary>
        ///   Gets or sets the workbook's calculation mode.
        /// </summary>
        public XLCalculateMode CalculateMode { get; set; }

        /// <summary>
        ///   Gets or sets the workbook's reference style.
        /// </summary>
        public XLReferenceStyle ReferenceStyle { get; set; }

        public IXLCustomProperties CustomProperties { get; private set; }

        public Boolean ShowFormulas { get; set; }
        public Boolean ShowGridLines { get; set; }
        public Boolean ShowOutlineSymbols { get; set; }
        public Boolean ShowRowColHeaders { get; set; }
        public Boolean ShowRuler { get; set; }
        public Boolean ShowWhiteSpace { get; set; }
        public Boolean ShowZeros { get; set; }
        public Boolean RightToLeft { get; set; }

        public Boolean DefaultShowFormulas
        {
            get { return false; }
        }

        public Boolean DefaultShowGridLines
        {
            get { return true; }
        }

        public Boolean DefaultShowOutlineSymbols
        {
            get { return true; }
        }

        public Boolean DefaultShowRowColHeaders
        {
            get { return true; }
        }

        public Boolean DefaultShowRuler
        {
            get { return true; }
        }

        public Boolean DefaultShowWhiteSpace
        {
            get { return true; }
        }

        public Boolean DefaultShowZeros
        {
            get { return true; }
        }

        public Boolean DefaultRightToLeft
        {
            get { return false; }
        }

        private void InitializeTheme()
        {
            Theme = new XLTheme
                        {
                            Text1 = XLColor.FromHtml("#FF000000"),
                            Background1 = XLColor.FromHtml("#FFFFFFFF"),
                            Text2 = XLColor.FromHtml("#FF1F497D"),
                            Background2 = XLColor.FromHtml("#FFEEECE1"),
                            Accent1 = XLColor.FromHtml("#FF4F81BD"),
                            Accent2 = XLColor.FromHtml("#FFC0504D"),
                            Accent3 = XLColor.FromHtml("#FF9BBB59"),
                            Accent4 = XLColor.FromHtml("#FF8064A2"),
                            Accent5 = XLColor.FromHtml("#FF4BACC6"),
                            Accent6 = XLColor.FromHtml("#FFF79646"),
                            Hyperlink = XLColor.FromHtml("#FF0000FF"),
                            FollowedHyperlink = XLColor.FromHtml("#FF800080")
                        };
        }

        internal IXLColor GetXLColor(XLThemeColor themeColor)
        {
            switch (themeColor)
            {
                case XLThemeColor.Text1:
                    return Theme.Text1;
                case XLThemeColor.Background1:
                    return Theme.Background1;
                case XLThemeColor.Text2:
                    return Theme.Text2;
                case XLThemeColor.Background2:
                    return Theme.Background2;
                case XLThemeColor.Accent1:
                    return Theme.Accent1;
                case XLThemeColor.Accent2:
                    return Theme.Accent2;
                case XLThemeColor.Accent3:
                    return Theme.Accent3;
                case XLThemeColor.Accent4:
                    return Theme.Accent4;
                case XLThemeColor.Accent5:
                    return Theme.Accent5;
                case XLThemeColor.Accent6:
                    return Theme.Accent6;
                default:
                    throw new ArgumentException("Invalid theme color");
            }
        }

        public IXLNamedRange NamedRange(String rangeName)
        {
            return NamedRanges.NamedRange(rangeName);
        }

        /// <summary>
        ///   Saves the current workbook.
        /// </summary>
        public void Save()
        {
            if (_loadSource == XLLoadSource.New)
                throw new Exception("This is a new file, please use one of the SaveAs methods.");

            if (_loadSource == XLLoadSource.Stream)
                CreatePackage(_originalStream, false);
            else
                CreatePackage(_originalFile);
        }

        /// <summary>
        ///   Saves the current workbook to a file.
        /// </summary>
        public void SaveAs(String file)
        {
            PathHelper.CreateDirectory(Path.GetDirectoryName(file));
            if (_loadSource == XLLoadSource.New)
            {
                if (File.Exists(file))
                    File.Delete(file);

                CreatePackage(file);
            }
            else if (_loadSource == XLLoadSource.File)
            {
                if (String.Compare(_originalFile.Trim(), file.Trim(), true) != 0)
                    File.Copy(_originalFile, file, true);

                CreatePackage(file);
            }
            else if (_loadSource == XLLoadSource.Stream)
            {
                _originalStream.Position = 0;

                using (var fileStream = File.Create(file))
                {
                    CopyStream(_originalStream, fileStream);
                    //fileStream.Position = 0;
                    CreatePackage(fileStream, false);
                    fileStream.Close();
                }
            }
        }

        /// <summary>
        ///   Saves the current workbook to a stream.
        /// </summary>
        public void SaveAs(Stream stream)
        {
            if (_loadSource == XLLoadSource.New)
                CreatePackage(stream, true);
            else if (_loadSource == XLLoadSource.File)
            {
                using (var fileStream = new FileStream(_originalFile, FileMode.Open))
                {
                    CopyStream(fileStream, stream);
                    fileStream.Close();
                }
                CreatePackage(stream, false);
            }
            else if (_loadSource == XLLoadSource.Stream)
            {
                _originalStream.Position = 0;
                if (_originalStream != stream)
                    CopyStream(_originalStream, stream);

                CreatePackage(stream, false);
            }
        }

        internal void CopyStream(Stream input, Stream output)
        {
            var buffer = new byte[8 * 1024];
            int len;
            while ((len = input.Read(buffer, 0, buffer.Length)) > 0)
                output.Write(buffer, 0, len);
        }

        public IXLWorksheet Worksheet(String name)
        {
            return WorksheetsInternal.Worksheet(name);
        }

        public IXLWorksheet Worksheet(Int32 position)
        {
            return WorksheetsInternal.Worksheet(position);
        }

        public IXLCustomProperty CustomProperty(String name)
        {
            return CustomProperties.CustomProperty(name);
        }

        public IXLCells FindCells(Func<IXLCell, Boolean> predicate)
        {
            var cells = new XLCells(false, false);
            foreach (XLWorksheet ws in WorksheetsInternal)
            {
                foreach (XLCell cell in ws.CellsUsed(true))
                {
                    if (predicate(cell))
                        cells.Add(cell);
                }
            }
            return cells;
        }

        public IXLRows FindRows(Func<IXLRow, Boolean> predicate)
        {
            var rows = new XLRows(null);
            foreach (XLWorksheet ws in WorksheetsInternal)
            {
                foreach (IXLRow row in ws.Rows().Where(predicate))
                    rows.Add(row as XLRow);
            }
            return rows;
        }

        public IXLColumns FindColumns(Func<IXLColumn, Boolean> predicate)
        {
            var columns = new XLColumns(null);
            foreach (XLWorksheet ws in WorksheetsInternal)
            {
                foreach (IXLColumn column in ws.Columns().Where(predicate))
                    columns.Add(column as XLColumn);
            }
            return columns;
        }

        #region Fields

        private readonly XLLoadSource _loadSource = XLLoadSource.New;
        private readonly String _originalFile;
        private readonly Stream _originalStream;

        #endregion

        #region Constructor

        /// <summary>
        ///   Creates a new Excel workbook.
        /// </summary>
        public XLWorkbook()
        {
            DefaultRowHeight = 15;
            DefaultColumnWidth = 8.43;
            Style = new XLStyle(null, DefaultStyle);
            RowHeight = DefaultRowHeight;
            ColumnWidth = DefaultColumnWidth;
            PageOptions = DefaultPageOptions;
            Outline = DefaultOutline;
            Properties = new XLWorkbookProperties();
            CalculateMode = XLCalculateMode.Default;
            ReferenceStyle = XLReferenceStyle.Default;
            InitializeTheme();
            ShowFormulas = DefaultShowFormulas;
            ShowGridLines = DefaultShowGridLines;
            ShowOutlineSymbols = DefaultShowOutlineSymbols;
            ShowRowColHeaders = DefaultShowRowColHeaders;
            ShowRuler = DefaultShowRuler;
            ShowWhiteSpace = DefaultShowWhiteSpace;
            ShowZeros = DefaultShowZeros;
            RightToLeft = DefaultRightToLeft;
            WorksheetsInternal = new XLWorksheets(this);
            NamedRanges = new XLNamedRanges(this);
            CustomProperties = new XLCustomProperties(this);
        }

        /// <summary>
        ///   Opens an existing workbook from a file.
        /// </summary>
        /// <param name = "file">The file to open.</param>
        public XLWorkbook(String file)
            : this()
        {
            _loadSource = XLLoadSource.File;
            _originalFile = file;
            Load(file);
        }

        /// <summary>
        ///   Opens an existing workbook from a stream.
        /// </summary>
        /// <param name = "stream">The stream to open.</param>
        public XLWorkbook(Stream stream)
            : this()
        {
            _loadSource = XLLoadSource.Stream;
            _originalStream = stream;
            Load(stream);
        }

        #endregion

        #region Nested type: UnsupportedSheet

        private sealed class UnsupportedSheet
        {
            public Boolean IsActive;
            public UInt32 SheetId;
        }

        #endregion

        public IXLCell Cell(String namedCell)
        {
            return NamedRange(namedCell).Ranges.First().FirstCell();
        }

        public IXLCells Cells(String namedCells)
        {
            return Ranges(namedCells).Cells();
        }

        public IXLRange Range(String namedRange)
        {
            return NamedRange(namedRange).Ranges.First();
        }

        public IXLRanges Ranges(String namedRanges)
        {
            var retVal = new XLRanges();
            var rangePairs = namedRanges.Split(',');
            foreach (string pair in rangePairs)
                retVal.Add(Range(pair.Trim()));
            return retVal;
        }
    }
}