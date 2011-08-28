//#define WITH_TEST_COMPONENT

//This is a precompiler directive.
//If you decomment the line above it will kick in
#if DEBUG && WITH_TEST_COMPONENT

using System;
using System.Diagnostics;
using Grasshopper;
using Grasshopper.Kernel;
using System.Windows.Forms;

namespace GrasshopperEvents
{
    public class TestForSafeComponent : SafeComponent
    {
        public TestForSafeComponent() :
            base("A test for what happens inside the safecomponent", "TEST", "Test", "Params", "Interop")
        {
            MessageBox.Show(@"Constructor called.
Normally Grasshopper creates two objects for each inserted component.",
                               "Initialize()");
        }

        protected override void Initialize(int aliveElements)
        {
            MessageBox.Show("Initialize called", "Initialize()");
            base.Initialize(aliveElements);
        }

        protected override void SolveInstance(IGH_DataAccess DA, bool secondOrLaterRun)
        {
            MessageBox.Show("SolveInstance() called", "SolveInstance()");
        }

        protected override void Dispose(bool disposing)
        {
            if (_doc != null)
            {

                MessageBox.Show(@"Dispose() on the Initialized object was called.
This should only appear once for each constructor call."
                , "Dispose()");

            }
            else
            {
                MessageBox.Show(@"This Dispose() is of little importance.
Nobody ever called the Initialize() on this", "Dispose()");
            }
            base.Dispose(disposing);
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
        }

        public override Guid ComponentGuid
        {
            get
            {
                return new Guid("{C1C11059-3402-1933-1890-4B2370CC5842}");
            }
        }
    }
}

#endif