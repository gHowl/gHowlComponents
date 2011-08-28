using System;
using System.Diagnostics;
using Grasshopper;
using Grasshopper.Kernel;

#if DEBUG
using System.Windows.Forms;
#endif

namespace gHowl
{
    /// <summary>
    /// This is an abstract class which automatically calls its own Dispose method when the user deletes it or
    /// closes the document. Hopefully, it handles all cases well, but please report if you spot any error.
    /// Please use the standard constructor if you need to execute something also when the assembly loads,
    /// and the Initialize() method to setup any single component when it first lands&executes on the canvas.
    /// You must call base.Dispose(disposing) if you override Dispose(bool).
    /// </summary>
    public abstract class SafeComponent : GH_Component, IGH_DocumentObject, IDisposable
    {
        protected GH_Document _doc;
        private static int _aliveCount = 0;
        private bool _secondOrLaterRun;
        private Guid _docGuid;
        private bool _disposed;
        private bool _locked;

        static SafeComponent()
        {
        }

        /// <summary>
        /// Do not use this constructor for initialization, but always use the Initialize() method, which will run only once.
        /// This constructor is called more times at startup for indexing the picture and some other external reasons.
        /// </summary>
        protected SafeComponent(string name, string abbreviation, string description, string category, string subCategory) :
            base(name, abbreviation, description, category, subCategory)
        {
            if (Grasshopper.Global_Proc.Version.major == 0 && Grasshopper.Global_Proc.Version.minor < 8)
            {
                throw new System.TypeLoadException("Grasshopper must be in version 0.8.01 or later" +
                    "This version of gHowl does not support any previous Grasshopper.");
            }
        }

        protected sealed override void SolveInstance(IGH_DataAccess DA)
        {
            if (_disposed)
                throw new ObjectDisposedException("This object is already disposed, either by the user deletion or becasue the document was closed.");

            if (!_secondOrLaterRun)
            {
                GH_Document d = OnPingDocument();
                if (d == null)
                    return;

                RegisterComponent(d);
                Initialize(_aliveCount);
            }

            SolveInstance(DA, _secondOrLaterRun);
        }

        protected abstract void SolveInstance(IGH_DataAccess DA, bool secondOrLaterRun);

        private void GrasshopperDocumentClosed(GH_DocumentServer sender, GH_Document doc)
        {
            if (doc != null && doc.DocumentID == _docGuid)
            {
                Dispose();
            }
        }

        protected void Reset()
        {
            _secondOrLaterRun = false;
        }

        private void GrasshopperObjectsDeleted(object sender, GH_DocObjectEventArgs e)
        {
            if (e != null && e.Attributes != null)
            {
                for (int i = 0; i < e.ObjectCount; i++)
                {
                    if (e.Attributes[i] != null && e.Attributes[i].InstanceGuid == this.InstanceGuid)
                    {
                        Dispose();
                    }
                    Debug.Assert(e.Attributes[i] != null, "e.Attributes[i] is null");
                }
            }
            Debug.Assert(e != null && e.Attributes != null, "e or e.Attributes is null");
        }

        private void RegisterComponent(GH_Document doc)
        {
            _secondOrLaterRun = true;
            _doc = doc;
            _docGuid = doc.DocumentID;

            doc.ObjectsDeleted += GrasshopperObjectsDeleted;
            GH_InstanceServer.DocumentServer.DocumentRemoved += GrasshopperDocumentClosed;
            doc.SolutionStart += AfterDocumentChanged;

            _aliveCount++;
        }

        void AfterDocumentChanged(object sender, GH_SolutionEventArgs args)
        {

            if (this.Locked != _locked)
            {
                _locked = this.Locked;
                OnLockedChanged(_locked);
            }
        }

        protected virtual void OnLockedChanged(bool nowIsLocked)
        {

        }

        private void DeregisterComponent()
        {
            if (_doc != null && _secondOrLaterRun)
            {
                _doc.ObjectsDeleted -= GrasshopperObjectsDeleted;

                if (GH_InstanceServer.DocumentServer != null)
                    GH_InstanceServer.DocumentServer.DocumentRemoved -= GrasshopperDocumentClosed;

               _doc.SolutionStart -= AfterDocumentChanged;
            }
            _aliveCount--;
        }

        /// <summary>
        /// Initializes the component when it first executes and at no other earlier or later time. It runs once for each component
        /// </summary>
        protected virtual void Initialize(int aliveElements)
        {
        }

        /// <summary>
        /// If you override this, be very sure you always call base.Dispose(disposing) or MyBase.Dispose(disposing) in
        /// Vb.Net from within your code.
        /// </summary>
        /// <param name="disposing">If disposing equals false, the method has been called by the 
        /// runtime from inside the finalizer and you should not reference other objects. Only unmanaged resources
        /// can be disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (!_disposed)
                {
                    DeregisterComponent();
                    _disposed = true;

                    if (disposing)
                    {
                        GC.SuppressFinalize(this);
                    }
                }
            }
#if DEBUG
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
#else
               catch
               {
               }
#endif

        }

        /// <summary>
        /// The IDisposable implementation. You do not normally need to call this.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        ~SafeComponent()
        {
            Dispose(false);
        }
    }
}