using System;
using System.Collections.Generic;
using System.Text;
using Grasshopper.Kernel;
using gHowl.Properties;

namespace gHowl
{
    public class gHowlData: GH_AssemblyInfo
    {
        public override System.Drawing.Bitmap AssemblyIcon
        {
            get
            {
                return Resources.howl;
            }
        }
        public override string AssemblyName
        {
            get
            {
                return "gHowl";
            }
        }
        public override string AssemblyVersion
        {
            get
            {

                return "r50";
            }
        }

        public override string AuthorContact
        {
            get
            {
                return "gHowlcomponents@gmail.com";
            }
        }
        public override string AssemblyDescription
        {
            get
            {
                return "gHowl is a set of interoperability components which extends the ability of Grasshopper to communicate with other applications.";
            }
        }

        public override string AuthorName
        {
            get
            {
                return "gHowl is written by its contributors. Till today, Damien Alomar, Luis Fraguada and Giulio Piacentino, in alphabetical order";
            }
        }

        public override GH_LibraryLicense AssemblyLicense
        {
            get
            {
                return GH_LibraryLicense.beta;
            }
        }
    }
}
