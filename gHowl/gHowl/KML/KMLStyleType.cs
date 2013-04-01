using System;
using System.Drawing;

using Grasshopper.Kernel.Types;
using GH_IO;
using GH_IO.Serialization;



namespace gHowl.KML
{
    public class KMLStyleType : IGH_Goo, GH_ISerializable
    {
        //Fields
        public string fillColor, lineColor, name;
        public double lineWidth;

        // Default Constructor, sets the state to Unknown.
        public KMLStyleType()
        {
            this.fillColor = ColorToHex(Color.White, 135);
            this.lineColor = ColorToHex(Color.Black, 255);
            this.lineWidth = 1.0;
            this.name = "";
        }

        // Constructor with initial value
        public KMLStyleType(Color fillC, Color lineC, double lineW, string text)
        {
            this.fillColor = ColorToHex(fillC);
            this.lineColor = ColorToHex(lineC);
            this.lineWidth = lineW;
            this.name = text;

        }

        public string ColorToHex(Color c)
        {
            return String.Format("{0:x2}{1:x2}{2:x2}{3:x2}", c.A, c.B, c.G, c.R);
        }

        public string ColorToHex(Color c, byte a)
        {
            return String.Format("{0:x2}{1:x2}{2:x2}{3:x2}", a, c.B, c.G, c.R);
        }


        // Copy Constructor
        public KMLStyleType(KMLStyleType KMLStyleSource)
        {
            this.fillColor = KMLStyleSource.fillColor;
            this.lineColor = KMLStyleSource.lineColor;
            this.lineWidth = KMLStyleSource.lineWidth;
            this.name = KMLStyleSource.name;
        }

       
        public bool CastFrom(object source)
        {
            return source.GetType().Equals(source.GetType());
        }

        public bool CastTo<T>(out T target)
        {
            target = default(T);
            return false;
        }

        public IGH_Goo Duplicate()
        {
            return (IGH_Goo)base.MemberwiseClone();
        }

        public IGH_GooProxy EmitProxy()
        {
            return null;
        }

        public bool IsValid
        {
            get { return true; }

        }

        public string IsValidWhyNot
        {
            get { return ""; }

        }

        public object ScriptVariable()
        {
            return this;
        }

        public string TypeDescription
        {
            get { return "KML Object Attributes (Fill Color, Line Color, Line Width, Name)"; }
        }

        public string TypeName
        {
            get { return "KMLStyle"; }
        }

        public bool Read(GH_IReader reader)
        {
            return true;
        }

        public bool Write(GH_IWriter writer)
        {
            return true;
        }

        public override string ToString()
        {
            return ("KML Style (" + this.fillColor+", "+this.lineColor+", "+this.lineWidth+","+this.name+")");
        }

 

    }
}
