using DotNetHelper.Cleaner.Interfaces;

namespace DotNetHelper.Cleaner.Policy.Folders
{
    public class FolderLastWriteTimeDeletePolicy : IFolderDeletePolicy
    {
        private readonly TimeSpan? _retentionTime;
        private readonly IReferenceTimeProvider _referenceTimeProvider;

        public FolderLastWriteTimeDeletePolicy(TimeSpan? retentionTime, IReferenceTimeProvider referenceTimeProvider)
        {
            _retentionTime = retentionTime;
            _referenceTimeProvider = referenceTimeProvider;
        }

        public bool ShouldDeleteFolder(string folderPath)
        {
            if (_retentionTime is null) return false;

            DateTime referenceTime = _referenceTimeProvider.GetReferenceTime();
            DateTime border = referenceTime.Add(-(TimeSpan)_retentionTime);

            return Directory.GetLastWriteTime(folderPath) < border;
        }
    }
}
