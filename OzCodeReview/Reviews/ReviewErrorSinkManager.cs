using Microsoft.VisualStudio.Shell.TableManager;

namespace OzCodeReview.Reviews
{
    internal class ReviewErrorSinkManager : IDisposable
    {
        public ReviewErrorSinkManager(ReviewErrorTagProvider reviewErrorTagProvider, ITableDataSink sink)
        {
            this.ReviewErrorTagProvider = reviewErrorTagProvider;
            this.ErrorTableSink = sink;

            this.ReviewErrorTagProvider.AddSinkManager(this);
        }

        public ReviewErrorTagProvider ReviewErrorTagProvider { get; }

        public ITableDataSink ErrorTableSink { get; }

        public void Dispose()
        {
            this.ReviewErrorTagProvider.RemoveSinkManager(this);
        }

        public void UpdateSink(ITableEntriesSnapshotFactory factory)
        {
            this.ErrorTableSink.FactorySnapshotChanged(factory);
        }

        public void AddErrorListFactory(ITableEntriesSnapshotFactory factory)
        {
            this.ErrorTableSink.AddFactory(factory);
        }


        public void RemoveErrorListFactory(ITableEntriesSnapshotFactory factory)
        {
            this.ErrorTableSink.RemoveFactory(factory);
        }
    }
}