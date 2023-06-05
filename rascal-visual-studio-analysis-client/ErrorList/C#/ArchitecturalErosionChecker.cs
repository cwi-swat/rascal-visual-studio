using Microsoft.VisualStudio.Language.StandardClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Controls;
using System.Windows.Threading;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace ArchitecturalErosionChecker
{
    public class ArchitecturalErosionChecker : IVsRunningDocTableEvents
    {
        private readonly ArchitecturalErosionCheckerProvider _provider;
        private readonly ITextBuffer _buffer;

        private ITextSnapshot _currentSnapshot;

        private readonly List<ArchitecturalErosionCheckerTagger> _activeTaggers = new List<ArchitecturalErosionCheckerTagger>();

        internal string FilePath;
        internal readonly ViolationFactory Factory;

        // RDT
        uint rdtCookie;
        RunningDocumentTable rdt;

        internal ArchitecturalErosionChecker(ArchitecturalErosionCheckerProvider provider, ITextView textView, ITextBuffer buffer)
        {
            _provider = provider;
            _buffer = buffer;
            _currentSnapshot = buffer.CurrentSnapshot;

            // Advise the RDT of this event sink.
            IOleServiceProvider sp =
                Package.GetGlobalService(typeof(IOleServiceProvider)) as IOleServiceProvider;
            if (sp == null) return;

            rdt = new RunningDocumentTable(new ServiceProvider(sp));
            if (rdt == null) return;

            rdtCookie = rdt.Advise(this);


            this.Factory = new ViolationFactory(this, new ViolationSnapshot(this.FilePath, 0));

            this.StartRascal();
        }

        internal void AddTagger(ArchitecturalErosionCheckerTagger tagger)
        {
            _activeTaggers.Add(tagger);

            if (_activeTaggers.Count == 1)
            {
                // First tagger created ... start doing stuff.
                _buffer.ChangedLowPriority += this.OnBufferChange;

                _provider.AddArchitecturalErosionChecker(this);
            }
        }

        internal void RemoveTagger(ArchitecturalErosionCheckerTagger tagger)
        {
            _activeTaggers.Remove(tagger);

            if (_activeTaggers.Count == 0)
            {
                _buffer.ChangedLowPriority -= this.OnBufferChange;

                _provider.RemoveArchitecturalErosionChecker(this);

                _buffer.Properties.RemoveProperty(typeof(ArchitecturalErosionChecker));
            }
        }

        private void OnBufferChange(object sender, TextContentChangedEventArgs e)
        {
            _currentSnapshot = e.After;

            var oldViolations = this.Factory.CurrentSnapshot;
            //var newViolations = new ViolationSnapshot(this.FilePath, oldSpenningErrors.VersionNumber + 1);
            var newViolations = new ViolationSnapshot(this.FilePath, oldViolations.VersionNumber);
            //var newViolations = oldViolations;

            this.UpdateViolations(newViolations);
        }

        #region IVsRunningDocTableEvents Members
        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }
        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }
        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }
        
        private bool toggle = true;
        public int OnAfterSave(uint docCookie)
        {
            this.FilePath = FindDocumentByCookie(docCookie);
            var oldViolations = this.Factory.CurrentSnapshot;
            var newViolations = new ViolationSnapshot(this.FilePath, oldViolations.VersionNumber + 1);


            if (toggle)
            {
                // experiment
                ITextSnapshot snap = _currentSnapshot;
                var snapshotSpan = new SnapshotSpan(snap, 1, 1);
                var testError = new Violation(snapshotSpan, "Fill in the specifics of the violation");
                newViolations.Errors.Add(testError);
                // experiment
            }
            this.UpdateViolations(newViolations);

            toggle = !toggle;

            return VSConstants.S_OK;
        }
        private async void StartRascal()
        {
            //var childProcess = new ProcessStartInfo("E:\\architectural-erosion\\rascal-visual-studio\\rascal-visual-studio-analysis-client\\ErrorList\\Server\\bin\\Debug\\Server.exe", "");
            var childProcess = new ProcessStartInfo("E:\\architectural-erosion\\ErrorList\\ConsoleApp\\bin\\Debug\\ConsoleApp.exe", "");
            childProcess.UseShellExecute = false;
            childProcess.RedirectStandardInput = true;
            childProcess.RedirectStandardOutput = true;
            var process = Process.Start(childProcess);
            //process.OutputDataReceived += new DataReceivedEventHandler(p_OutputDataReceived);
            //StreamWriter inputWriter = process.StandardInput;
            //inputWriter.Write("bla\n\r"); // Is redirected so will not be printed!!
            //inputWriter.Flush();
            //while (true) ; // find better solution!!!!


            //var stdioStream = FullDuplexStream.Splice(process.StandardOutput.BaseStream, process.StandardInput.BaseStream);
            //await ActAsRpcClientAsync(stdioStream);
        }
        //private async Task ActAsRpcClientAsync(Stream stream)
        //{
        //    Console.WriteLine("Connected. Sending request...");
        //    var jsonRpc = JsonRpc.Attach(stream);
        //    int sum = await jsonRpc.InvokeAsync<int>("Add", 3, 5);
        //    Console.WriteLine($"3 + 5 = {sum}");
        //}
        void p_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Debug.WriteLine(e.Data);
        }
        private string FindDocumentByCookie(uint docCookie)
        {
            return rdt.GetDocumentInfo(docCookie).Moniker;
        }
        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }
        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }
        #endregion

        private void UpdateViolations(ViolationSnapshot violation)
        {
            // Tell our factory to snap to a new snapshot.
            this.Factory.UpdateErrors(violation);

            // Tell the provider to mark all the sinks dirty (so, as a side-effect, they will start an update pass that will get the new snapshot
            // from the factory).
            _provider.UpdateAllSinks();

            foreach (var tagger in _activeTaggers)
            {
                tagger.UpdateErrors(_currentSnapshot, violation);
            }

            this.LastViolation = violation;
        }

        internal ViolationSnapshot LastViolation { get; private set; }
    }
}
