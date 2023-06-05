using Microsoft.VisualStudio.Shell.TableManager;

namespace ArchitecturalErosionChecker
{
    class ViolationFactory : TableEntriesSnapshotFactoryBase
    {
        private readonly ArchitecturalErosionChecker _architecturalErosionChecker;

        public ViolationSnapshot CurrentSnapshot { get; private set; }

        public ViolationFactory(ArchitecturalErosionChecker architecturalErosionChecker, ViolationSnapshot violation)
        {
            _architecturalErosionChecker = architecturalErosionChecker;

            this.CurrentSnapshot = violation;
        }

        internal void UpdateErrors(ViolationSnapshot violation)
        {
            this.CurrentSnapshot.NextSnapshot = violation;
            this.CurrentSnapshot = violation;
        }

        #region ITableEntriesSnapshotFactory members
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
        #endregion
    }
}
