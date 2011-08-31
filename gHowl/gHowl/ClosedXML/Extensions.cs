﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

[assembly: CLSCompliantAttribute(true)]
namespace ClosedXML.Excel
{
    public static class Extensions
    {
        // Adds the .ForEach method to all IEnumerables
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            foreach (T item in source)
                action(item);
        }

        private static readonly char[] hexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F'};

        public static String ToHex(this Color color)
        {

            byte[] bytes = new byte[4];

            bytes[0] = color.A;

            bytes[1] = color.R;

            bytes[2] = color.G;

            bytes[3] = color.B;

            char[] chars = new char[bytes.Length * 2];

            for (int i = 0; i < bytes.Length; i++)
            {

                int b = bytes[i];

                chars[i * 2] = hexDigits[b >> 4];

                chars[i * 2 + 1] = hexDigits[b & 0xF];

            }

            return new string(chars);

        }

        public static String RemoveSpecialCharacters(this String str)
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                //if ((c >= '0' && c <= '9') || (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z') | c == '.' || c == '_')
                if (Char.IsLetterOrDigit(c))
                {
                    sb.Append(c);
                }
            }
            return sb.ToString();
        }

        public static Int32 CharCount(this String instance, Char c)
        {
            return instance.Length - instance.Replace(c.ToString(), "").Length;
        }

        public static Boolean HasDuplicates<T>(this IEnumerable<T> source)
        {
            HashSet<T> distinctItems = new HashSet<T>();
            foreach (var item in source)
            {
                if (distinctItems.Contains(item))
                    return true;
                else
                    distinctItems.Add(item);
            }
            return false;
        }
    }

    public static class DictionaryExtensions
    {
        public static void RemoveAll<TKey, TValue>(this Dictionary<TKey, TValue> dic,
            Func<TValue, bool> predicate)
        {
            var keys = dic.Keys.Where(k => predicate(dic[k])).ToList();
            foreach (var key in keys)
            {
                dic.Remove(key);
            }
        }
    }

    public static class StringExtensions
    {
        public static bool IsNullOrWhiteSpace(string value)
        {
            if (value != null)
            {
                var length = value.Length;
                for (int i = 0; i < length; i++)
                {
                    if (!char.IsWhiteSpace(value[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private static readonly Regex RegexNewLine = new Regex(@"((?<!\r)\n|\r\n)");
        public static String FixNewLines(this String value)
        {
            return value.Contains("\n") ? RegexNewLine.Replace(value, Environment.NewLine) : value;
        }

        public static Boolean PreserveSpaces(this String value)
        {
            return value.StartsWith(" ") || value.EndsWith(" ") || value.Contains(Environment.NewLine);
        }
    }

    public static class DateTimeExtensions
    {
        public static Double MaxOADate
        {
            get
            {
                return 2958465.99999999;
            }
        }
    }

    public static class IntegerExtensions
    {
        private static readonly NumberFormatInfo nfi = CultureInfo.InvariantCulture.NumberFormat;
        private static readonly Dictionary<Int32, String> intToString = new Dictionary<int, string>();
        public static String ToStringLookup(this Int32 value)
        {
            if (!intToString.ContainsKey(value))
            {
                intToString.Add(value, value.ToString(nfi));
            }
            return intToString[value];
        }
    }

    public static class FontBaseExtensions
    {
        public static Double GetWidth(this IXLFontBase font, String text)
        {
            if (StringExtensions.IsNullOrWhiteSpace(text))
                return 0;

            var stringFont = new Font(font.FontName, (float)font.FontSize);

            var textSize = TextRenderer.MeasureText(text, stringFont);
            double width = (double)(((textSize.Width / (double)7) * 256) - (128 / 7)) / 256;
            width = (double)decimal.Round((decimal)width + 0.2M, 2);

            return width ;
        }

        public static Double GetHeight(this IXLFontBase font)
        {
            var stringFont = new Font(font.FontName, (float)font.FontSize);
            var textSize = TextRenderer.MeasureText("X", stringFont);
            return (double)textSize.Height * 0.85;
        }

        public static void CopyFont(this IXLFontBase font, IXLFontBase sourceFont)
        {
            font.Bold = sourceFont.Bold;
            font.Italic = sourceFont.Italic;
            font.Underline = sourceFont.Underline;
            font.Strikethrough = sourceFont.Strikethrough;
            font.VerticalAlignment = sourceFont.VerticalAlignment;
            font.Shadow = sourceFont.Shadow;
            font.FontSize = sourceFont.FontSize;
            font.FontColor = new XLColor(sourceFont.FontColor);
            font.FontName = sourceFont.FontName;
            font.FontFamilyNumbering = sourceFont.FontFamilyNumbering;
        }
    }
}

