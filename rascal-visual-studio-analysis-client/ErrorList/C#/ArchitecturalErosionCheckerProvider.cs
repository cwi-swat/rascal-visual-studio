using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using MsVsShell = Microsoft.VisualStudio.Shell;
using IOleServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace ArchitecturalErosionChecker
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Analyzable)]
    internal sealed class ArchitecturalErosionCheckerProvider : IViewTaggerProvider, ITableDataSource
    {
        internal readonly ITableManager ErrorTableManager;
        internal readonly ITextDocumentFactoryService TextDocumentFactoryService;
        internal readonly IClassifierAggregatorService ClassifierAggregatorService;

        const string _architecturalErosionCheckerDataSource = "ArchitecturalErosionChecker";

        private readonly List<SinkManager> _managers = new List<SinkManager>();      // Also used for locks
        private readonly List<ArchitecturalErosionChecker> _architecturalErosionChecker = new List<ArchitecturalErosionChecker>();

        // test
        private IVsGeneratorProgress codeGeneratorProgress;

        [ImportingConstructor]
        internal ArchitecturalErosionCheckerProvider([Import]ITableManagerProvider provider, [Import] ITextDocumentFactoryService textDocumentFactoryService, [Import] IClassifierAggregatorService classifierAggregatorService)
        {
            this.ErrorTableManager = provider.GetTableManager(StandardTables.ErrorsTable);
            this.TextDocumentFactoryService = textDocumentFactoryService;

            this.ClassifierAggregatorService = classifierAggregatorService;

            this.ErrorTableManager.AddSource(this, StandardTableColumnDefinitions.DetailsExpander, 
                                                   StandardTableColumnDefinitions.ErrorSeverity, StandardTableColumnDefinitions.ErrorCode,
                                                   StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.BuildTool,
                                                   StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.ErrorCategory,
                                                   StandardTableColumnDefinitions.Text, StandardTableColumnDefinitions.DocumentName, StandardTableColumnDefinitions.Line, StandardTableColumnDefinitions.Column);
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            ITagger<T> tagger = null;
            if ((buffer == textView.TextBuffer) && (typeof(T) == typeof(IErrorTag)))
            {
                var architecturalErosionChecker = buffer.Properties.GetOrCreateSingletonProperty(typeof(ArchitecturalErosionChecker), () => new ArchitecturalErosionChecker(this, textView, buffer));
                tagger = new ArchitecturalErosionCheckerTagger(architecturalErosionChecker) as ITagger<T>;
            }

            return tagger;
        }

        public string DisplayName
        {
            get
            {
                return "ArchitecturalErosionChecker";
            }
        }

        public string Identifier
        {
            get
            {
                return _architecturalErosionCheckerDataSource;
            }
        }

        public string SourceTypeIdentifier
        {
            get
            {
                return StandardTableDataSources.ErrorTableDataSource;
            }
        }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            // This method is called to each consumer interested in errors. In general, there will be only a single consumer (the error list tool window)
            // but it is always possible for 3rd parties to write code that will want to subscribe.
            return new SinkManager(this, sink);
        }

        public void AddSinkManager(SinkManager manager)
        {
            // This call can, in theory, happen from any thread so be appropriately thread safe.
            // In practice, it will probably be called only once from the UI thread (by the error list tool window).
            lock (_managers)
            {
                _managers.Add(manager);
                foreach (var architecturalErosionChecker in _architecturalErosionChecker)
                {
                    manager.AddArchitecturalErosionChecker(architecturalErosionChecker);
                }
            }
        }

        public void RemoveSinkManager(SinkManager manager)
        {
            // This call can, in theory, happen from any thread so be appropriately thread safe.
            // In practice, it will probably be called only once from the UI thread (by the error list tool window).
            lock (_managers)
            {
                _managers.Remove(manager);
            }
        }

        public void AddArchitecturalErosionChecker(ArchitecturalErosionChecker architecturalErosionChecker)
        {
            // This call will always happen on the UI thread (it is a side-effect of adding or removing the 1st/last tagger).
            lock (_managers)
            {
                _architecturalErosionChecker.Add(architecturalErosionChecker);
                foreach (var manager in _managers)
                {
                    manager.AddArchitecturalErosionChecker(architecturalErosionChecker);
                }
            }
        }

        public void RemoveArchitecturalErosionChecker(ArchitecturalErosionChecker architecturalErosionChecker)
        {
            // This call will always happen on the UI thread (it is a side-effect of adding or removing the 1st/last tagger).
            lock (_managers)
            {
                _architecturalErosionChecker.Remove(architecturalErosionChecker);

                foreach (var manager in _managers)
                {
                    manager.RemoveArchitecturalErosionChecker(architecturalErosionChecker);
                }
            }
        }

        public void UpdateAllSinks()
        {
            lock (_managers)
            {
                foreach (var manager in _managers)
                {
                    manager.UpdateSink();
                }
            }
        }

    }
}
