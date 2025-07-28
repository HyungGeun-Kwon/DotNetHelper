using System;
using DotNetHelper.Cleaner.Interfaces;
using DotNetHelper.Cleaner.Policy.Files;
using DotNetHelper.Cleaner.Policy.Folders;
using DotNetHelper.Cleaner.Provider;
using DotNetHelper.Cleaner.Services;

namespace DotNetHelper.Cleaner.Builders
{
    /// <summary>
    /// 현재 시간(DateTime.Now) 기준 (NowReferenceTimeProvider)
    /// 파일 폴더는 생성 시간 기준 삭제 (FileCreationTimeDeletePolicy/FolderCreationTimeDeletePolicy)
    /// </summary>
    public class DefaultCleanerBuilder : ICleanerBuilder
    {
        public ICleaner Build(TimeSpan? retentionTime)
        {
            IReferenceTimeProvider referenceTimeProvider = new NowReferenceTimeProvider();
            
            IFileDeletePolicy filePolicy = new FileCreationTimeDeletePolicy(retentionTime, referenceTimeProvider);
            IFolderDeletePolicy folderPolicy = new FolderCreationTimeDeletePolicy(retentionTime, referenceTimeProvider);

            return new FileSystemCleaner(filePolicy, folderPolicy);
        }
    }
}
