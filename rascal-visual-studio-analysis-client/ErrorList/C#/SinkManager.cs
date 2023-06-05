using Microsoft.VisualStudio.Shell.TableManager;
using System;

namespace ArchitecturalErosionChecker
{
    /// <summary>
    /// Every consumer of data from an <see cref="ITableDataSource"/> provides an <see cref="ITableDataSink"/> to record the changes. We give the consumer
    /// an IDisposable (this object) that they hang on to as long as they are interested in our data (and they Dispose() of it when they are done).
    /// </summary>
    class SinkManager : IDisposable
    {
        private readonly ArchitecturalErosionCheckerProvider _architecturalErosionCheckerProvider;
        private readonly ITableDataSink _sink;

        internal SinkManager(ArchitecturalErosionCheckerProvider architecturalErosionCheckerProvider, ITableDataSink sink)
        {
            _architecturalErosionCheckerProvider = architecturalErosionCheckerProvider;
            _sink = sink;

            architecturalErosionCheckerProvider.AddSinkManager(this);
        }

        public void Dispose()
        {
            // Called when the person who subscribed to the data source disposes of the cookie (== this object) they were given.
            _architecturalErosionCheckerProvider.RemoveSinkManager(this);
        }

        internal void AddArchitecturalErosionChecker(ArchitecturalErosionChecker architecturalErosionChecker)
        {
            _sink.AddFactory(architecturalErosionChecker.Factory);
        }

        internal void RemoveArchitecturalErosionChecker(ArchitecturalErosionChecker architecturalErosionChecker)
        {
            _sink.RemoveFactory(architecturalErosionChecker.Factory);
        }

        internal void UpdateSink()
        {
            _sink.FactorySnapshotChanged(null);
        }
    }
}
