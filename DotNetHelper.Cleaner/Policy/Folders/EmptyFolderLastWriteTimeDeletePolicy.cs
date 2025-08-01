using DotNetHelper.Cleaner.Interfaces;

namespace DotNetHelper.Cleaner.Policy.Folders
{
    public class EmptyFolderLastWriteTimeDeletePolicy : IFolderDeletePolicy
    {
        private readonly TimeSpan? _retentionTime;
        private readonly IReferenceTimeProvider _referenceTimeProvider;

        public EmptyFolderLastWriteTimeDeletePolicy(TimeSpan? retentionTime, IReferenceTimeProvider referenceTimeProvider)
        {
            _retentionTime = retentionTime;
            _referenceTimeProvider = referenceTimeProvider;
        }

        public bool ShouldDeleteFolder(string folderPath)
        {
            if (_retentionTime is null) return false;

            if (Directory.EnumerateFileSystemEntries(folderPath).Any())
            {
                // 비어있지 않다면 삭제하지 않음.
                return false;
            }
            DateTime referenceTime = _referenceTimeProvider.GetReferenceTime();
            DateTime border = referenceTime.Add(-(TimeSpan)_retentionTime);

            return Directory.GetLastWriteTime(folderPath) < border;
        }
    }
}
