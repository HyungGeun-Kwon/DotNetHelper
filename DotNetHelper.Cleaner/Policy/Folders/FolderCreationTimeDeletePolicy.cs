using System;
using System.IO;
using DotNetHelper.Cleaner.Interfaces;

namespace DotNetHelper.Cleaner.Policy.Folders
{
    public class FolderCreationTimeDeletePolicy : IFolderDeletePolicy
    {
        private readonly TimeSpan? _retentionTime;
        private readonly IReferenceTimeProvider _referenceTimeProvider;

        public FolderCreationTimeDeletePolicy(TimeSpan? retentionTime, IReferenceTimeProvider referenceTimeProvider)
        {
            _retentionTime = retentionTime;
            _referenceTimeProvider = referenceTimeProvider;
        }

        public bool ShouldDeleteFolder(string folderPath)
        {
            if (_retentionTime is null) return false;

            DateTime referenceTime = _referenceTimeProvider.GetReferenceTime();
            DateTime border = referenceTime.Add(-(TimeSpan)_retentionTime);

            return Directory.GetCreationTime(folderPath) < border;
        }
    }
}
