﻿using System;

namespace ClosedXML.Excel
{
    public enum XLFillPatternValues
    {
        DarkDown,
        DarkGray,
        DarkGrid,
        DarkHorizontal,
        DarkTrellis,
        DarkUp,
        DarkVertical,
        Gray0625,
        Gray125,
        LightDown,
        LightGray,
        LightGrid,
        LightHorizontal,
        LightTrellis,
        LightUp,
        LightVertical,
        MediumGray,
        None,
        Solid
    }

    public interface IXLFill:IEquatable<IXLFill>
    {
        IXLColor BackgroundColor { get; set; }
        IXLColor PatternColor { get; set; }
        IXLColor PatternBackgroundColor { get; set; }
        XLFillPatternValues PatternType { get; set; }

        IXLStyle SetBackgroundColor(IXLColor value);
        IXLStyle SetPatternColor(IXLColor value);
        IXLStyle SetPatternBackgroundColor(IXLColor value);
        IXLStyle SetPatternType(XLFillPatternValues value);

    }
}
