using System;
using System.IO;
using DotNetHelper.Cleaner.Interfaces;

namespace DotNetHelper.Cleaner.Policy.Files
{
    public class FileCreationTimeDeletePolicy : IFileDeletePolicy
    {
        private readonly TimeSpan? _retentionTime;
        private readonly IReferenceTimeProvider _referenceTimeProvider; 

        public FileCreationTimeDeletePolicy(TimeSpan? retentionTime, IReferenceTimeProvider referenceTimeProvider)
        {
            _retentionTime = retentionTime;
            _referenceTimeProvider = referenceTimeProvider;
        }

        public bool ShouldDeleteFile(string filePath)
        {
            if (_retentionTime is null) return false;

            DateTime referenceTime = _referenceTimeProvider.GetReferenceTime();
            DateTime border = referenceTime.Add(-(TimeSpan)_retentionTime);

            return File.GetCreationTime(filePath) < border;
        }
    }
}
