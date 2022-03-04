using Microsoft.VisualStudio.Shell.TableControl;
using Microsoft.VisualStudio.Shell.TableManager;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;

namespace OzCodeReview.Reviews
{
    [Export(typeof(IViewTaggerProvider))]
    [TagType(typeof(IErrorTag))]
    [ContentType("text")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [TextViewRole(PredefinedTextViewRoles.Analyzable)]
    internal sealed class ReviewErrorTagProvider : IViewTaggerProvider, ITableDataSource
    {
        private const string ozCodeReviewDataSource = "SpellChecker";

        private readonly List<ReviewErrorsFactory> errorListFactories = new List<ReviewErrorsFactory>();

        private readonly List<ReviewErrorSinkManager> reviewErrorSinkManagers = new List<ReviewErrorSinkManager>();

        [ImportingConstructor]
        public ReviewErrorTagProvider([Import] ITableManagerProvider provider, [Import] ReviewTagsUtilities reviewTagsUtilities)
        {
            this.Provider = provider;
            this.ReviewTagsUtilities = reviewTagsUtilities;

            // TODO next version

            //this.ErrorTableManager = provider.GetTableManager(StandardTables.ErrorsTable);

            //this.ErrorTableManager.AddSource(this, StandardTableColumnDefinitions.DetailsExpander,
            //                                      StandardTableColumnDefinitions.ErrorSeverity, StandardTableColumnDefinitions.ErrorCode,
            //                                      StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.BuildTool,
            //                                      StandardTableColumnDefinitions.ErrorSource, StandardTableColumnDefinitions.ErrorCategory,
            //                                      StandardTableColumnDefinitions.Text, StandardTableColumnDefinitions.DocumentName, StandardTableColumnDefinitions.Line, StandardTableColumnDefinitions.Column);
        }

        public string DisplayName
        {
            get
            {
                return "OzCode review";
            }
        }

        public ITableManager ErrorTableManager { get; }

        public string Identifier
        {
            get
            {
                return ozCodeReviewDataSource;
            }
        }

        public ITableManagerProvider Provider { get; }

        public ReviewTagsUtilities ReviewTagsUtilities { get; }

        public string SourceTypeIdentifier
        {
            get
            {
                return StandardTableDataSources.ErrorTableDataSource;
            }
        }

        public void AddSinkManager(ReviewErrorSinkManager manager)
        {
            // This call can, in theory, happen from any thread so be appropriately thread safe.
            // In practice, it will probably be called only once from the UI thread (by the error list tool window).
            lock (this.reviewErrorSinkManagers)
            {
                this.reviewErrorSinkManagers.Add(manager);

                foreach (var errorFactory in this.errorListFactories)
                {
                    manager.AddErrorListFactory(errorFactory);
                }
            }
        }

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            if (textView.TextBuffer != buffer)
            {
                return null;
            }

            string filePath = null;

            string solutionDir = Path.GetDirectoryName(VS.Solutions.GetCurrentSolution().FullPath);

            if (buffer.Properties.TryGetProperty(typeof(ITextDocument), out ITextDocument textDocument))
            {
                filePath = textDocument.FilePath;
            }

            ReviewErrorsFactory factory = null;

            lock (this.errorListFactories)
            {
                var existing = errorListFactories.FirstOrDefault(f => f.FilePath == filePath);

                if (existing==null)
                {

                    factory = new ReviewErrorsFactory(solutionDir, this, filePath);

                    this.AddErrorListFactory(factory);
                }
                else
                {
                    factory = existing;
                }
            }

            return new ReviewErrorTagger(filePath, buffer.CurrentSnapshot, this.ReviewTagsUtilities, factory) as ITagger<T>;
        }

        public IDisposable Subscribe(ITableDataSink sink)
        {
            return new ReviewErrorSinkManager(this, sink);
        }
        public void UpdateAllSinks(ITableEntriesSnapshotFactory factory)
        {
            lock (this.reviewErrorSinkManagers)
            {
                foreach (var manager in this.reviewErrorSinkManagers)
                {
                    manager.UpdateSink(factory);
                }
            }
        }

        internal void RemoveSinkManager(ReviewErrorSinkManager reviewErrorSinkManager)
        {
            lock (this.reviewErrorSinkManagers)
            {
                this.reviewErrorSinkManagers.Remove(reviewErrorSinkManager);
            }
        }

        internal void AddErrorListFactory(ReviewErrorsFactory factory)
        {
            lock (this.reviewErrorSinkManagers)
            {
                this.errorListFactories.Add(factory);

                // Tell the preexisting sinks about the new error source
                foreach (var manager in this.reviewErrorSinkManagers)
                {
                    manager.AddErrorListFactory(factory);
                }
            }
        }

        public void RemoveErrorListFactory(ReviewErrorsFactory factory)
        {
            lock (this.reviewErrorSinkManagers)
            {
                this.errorListFactories.Remove(factory);

                foreach (var manager in this.reviewErrorSinkManagers)
                {
                    manager.RemoveErrorListFactory(factory);
                }
            }
        }
    }
}