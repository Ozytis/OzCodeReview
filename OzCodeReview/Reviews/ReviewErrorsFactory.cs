
using Community.VisualStudio.Toolkit;

using Microsoft.VisualStudio.Shell.TableManager;

using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;

namespace OzCodeReview.Reviews
{
    public class ReviewErrorsFactory : TableEntriesSnapshotFactoryBase
    {
        private static List<ReviewErrorTag> currentErrors = new();

        internal ReviewErrorsFactory(string solutionDir, ReviewErrorTagProvider errorProvider)
        {
            this.SolutionDir = solutionDir;
            this.ErrorProvider = errorProvider;
            this.CurrentSnapshot = new ReviewErrorSnapshot(0, currentErrors, this.SolutionDir);
        }

        public string SolutionDir { get; }
       
        internal ReviewErrorTagProvider ErrorProvider { get; }

        internal ReviewErrorSnapshot CurrentSnapshot { get; private set; }


        internal void UpdateErrors(ReviewErrorSnapshot errorsList)
        {
            CurrentSnapshot = errorsList;
        }

        internal void AddErrorItem(ReviewErrorTag newError)
        {
            lock (currentErrors)
            {
                currentErrors.Add(newError);
                currentErrors = currentErrors.GroupBy(e => e.Review.Id).Select(k => k.First()).ToList();
            }

            this.UpdateErrors(new ReviewErrorSnapshot(this.CurrentSnapshot.VersionNumber + 1, currentErrors, this.SolutionDir));
        }

        internal void AddErrorItems(IEnumerable<ReviewErrorTag> newErrors)
        {
            lock (currentErrors)
            {
                currentErrors.AddRange(newErrors);
                currentErrors = currentErrors.GroupBy(e => e.Review.Id).Select(k => k.First()).ToList();
            }

            UpdateErrors(new ReviewErrorSnapshot(this.CurrentSnapshot.VersionNumber + 1, currentErrors, this.SolutionDir));
        }

        public override int CurrentVersionNumber
        {
            get
            {
                return this.CurrentSnapshot.VersionNumber;
            }
        }

        public override void Dispose()
        {
        }

        public override ITableEntriesSnapshot GetCurrentSnapshot()
        {
            return this.CurrentSnapshot;
        }

        public override ITableEntriesSnapshot GetSnapshot(int versionNumber)
        {
            // In theory the snapshot could change in the middle of the return statement so snap the snapshot just to be safe.
            var snapshot = this.CurrentSnapshot;
            return (versionNumber == snapshot.VersionNumber) ? snapshot : null;
        }
    }
}
