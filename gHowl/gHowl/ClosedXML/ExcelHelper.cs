using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;

namespace ClosedXML.Excel
{
    using System.Linq;

    /// <summary>
    ///   Common methods
    /// </summary>
    internal static class ExcelHelper
    {
        public const int MinRowNumber = 1;
        public const int MinColumnNumber = 1;
        public const int MaxRowNumber = 1048576;
        public const int MaxColumnNumber = 16384;

        private const Int32 TwoT26 = 26 * 26;
        internal static readonly NumberFormatInfo NumberFormatForParse = CultureInfo.InvariantCulture.NumberFormat;

        /// <summary>
        ///   Gets the column number of a given column letter.
        /// </summary>
        /// <param name = "columnLetter">The column letter to translate into a column number.</param>
        public static int GetColumnNumberFromLetter(string columnLetter)
        {
            if (columnLetter[0] <= '9')
                return Int32.Parse(columnLetter, NumberFormatForParse);

            columnLetter = columnLetter.ToUpper();
            int length = columnLetter.Length;
            if (length == 1)
                return Convert.ToByte(columnLetter[0]) - 64;
            if (length == 2)
            {
                return
                    ((Convert.ToByte(columnLetter[0]) - 64) * 26) +
                    (Convert.ToByte(columnLetter[1]) - 64);
            }
            if (length == 3)
            {
                return ((Convert.ToByte(columnLetter[0]) - 64) * TwoT26) +
                       ((Convert.ToByte(columnLetter[1]) - 64) * 26) +
                       (Convert.ToByte(columnLetter[2]) - 64);
            }
            throw new ApplicationException("Column Length must be between 1 and 3.");
        }

        /// <summary>
        ///   Gets the column letter of a given column number.
        /// </summary>
        /// <param name = "column">The column number to translate into a column letter.</param>
        public static string GetColumnLetterFromNumber(int column)
        {
            #region Check

            if (column <= 0)
                throw new ArgumentOutOfRangeException("column", "Must be more than 0");

            #endregion

            var value = new StringBuilder(6);
            while (column > 0)
            {
                int residue = column % 26;
                column /= 26;
                if (residue == 0)
                {
                    residue = 26;
                    column--;
                }
                value.Insert(0, (char)(64 + residue));
            }
            return value.ToString();
        }

        public static bool IsValidColumn(string column)
        {
            if (StringExtensions.IsNullOrWhiteSpace(column) || column.Length > 3)
                return false;

            String theColumn = column.ToUpper();
            return
                !column.Where((t, i) => theColumn[i] < 'A' || theColumn[i] > 'Z' || (i == 2 && theColumn[i] > 'D')).Any();
        }

        public static bool IsValidRow(string rowString)
        {
            Int32 row;
            if (Int32.TryParse(rowString, out row))
                return row > 0 && row <= MaxRowNumber;
            return false;
        }

        public static bool IsValidA1Address(string address)
        {
            if (StringExtensions.IsNullOrWhiteSpace(address))
                return false;

            address = address.Replace("$", "");
            Int32 rowPos = 0;
            Int32 addressLength = address.Length;
            while (rowPos < addressLength && (address[rowPos] > '9' || address[rowPos] < '0'))
                rowPos++;

            return
                rowPos < addressLength
                && IsValidRow(address.Substring(rowPos))
                && IsValidColumn(address.Substring(0, rowPos));
        }

        public static Boolean IsValidRangeAddress(String rangeAddress)
        {
            if (StringExtensions.IsNullOrWhiteSpace(rangeAddress))
                return false;
        
            string addressToUse = rangeAddress.Contains("!")
                                      ? rangeAddress.Substring(rangeAddress.IndexOf("!") + 1)
                                      : rangeAddress;

            if (addressToUse.Contains(':'))
            {
                var arrRange = addressToUse.Split(':');
                string firstPart = arrRange[0];
                string secondPart = arrRange[1];
                return IsValidA1Address(firstPart) && IsValidA1Address(secondPart);
            }

            return IsValidA1Address(addressToUse);
        }

        public static int GetRowFromAddress1(string cellAddressString)
        {
            Int32 rowPos = 1;
            while (cellAddressString[rowPos] > '9')
                rowPos++;

            return Int32.Parse(cellAddressString.Substring(rowPos), NumberFormatForParse);
        }

        public static int GetColumnNumberFromAddress1(string cellAddressString)
        {
            Int32 rowPos = 0;
            while (cellAddressString[rowPos] > '9')
                rowPos++;

            return GetColumnNumberFromLetter(cellAddressString.Substring(0, rowPos));
        }

        public static int GetRowFromAddress2(string cellAddressString)
        {
            Int32 rowPos = 1;
            while (cellAddressString[rowPos] > '9')
                rowPos++;

            return
                Int32.Parse(
                    cellAddressString[rowPos] == '$'
                        ? cellAddressString.Substring(rowPos + 1)
                        : cellAddressString.Substring(rowPos), NumberFormatForParse);
        }

        public static int GetColumnNumberFromAddress2(string cellAddressString)
        {
            int startPos = cellAddressString[0] == '$' ? 1 : 0;

            Int32 rowPos = startPos;
            while (cellAddressString[rowPos] > '9')
                rowPos++;

            return
                GetColumnNumberFromLetter(cellAddressString[rowPos] == '$'
                                              ? cellAddressString.Substring(startPos, rowPos - 1)
                                              : cellAddressString.Substring(startPos, rowPos));
        }

        public static string[] SplitRange(string range)
        {
            return range.Contains('-') ? range.Replace('-', ':').Split(':') : range.Split(':');
        }


    }
}