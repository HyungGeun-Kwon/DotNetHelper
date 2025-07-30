using DotNetHelper.Cleaner.Interfaces;
using DotNetHelper.Cleaner.Policy.Files;
using DotNetHelper.Cleaner.Policy.Folders;
using DotNetHelper.Cleaner.Provider;
using DotNetHelper.Cleaner.Services;

namespace DotNetHelper.Logger.Bulders
{
    /// <summary>
    /// 00:00분 (DateTime.Now.Date) 기준 (TodayMidnightReferenceTimeProvider)
    /// 
    /// 파일은 수정 시간 기준 삭제 (FileLastWriteTimeDeletePolicy)
    ///     파일은 수정된 경우는 이슈가 발생하여 확인중에 저장되었을 확률이 있으므로
    ///     이슈 발생 시점으로부터 설정한 값까지 보증한다 라는 개념으로 LastWriteTime을 기준으로 삭제
    ///     CreationTime을 사용할 경우 파일을 주단위, 월단위 등 Day보다 더 높은 단위로 저장할 경우 문제될 수 있음.
    ///     
    /// 폴더는 생성 시간 + Empty여부 기준 삭제 (EmptyFolderCreationTimeDeletePolicy)
    ///     폴더는 IfEmpty 라는 조건도 있기때문에 생성 기준으로 삭제 진행.
    ///     LastWriteTime을 사용할 경우 유저가 해당 폴더에 필요없는 동작을 진행했을 경우 업데이트되어 삭제되지 않을 수 있음.
    ///     
    /// </summary>
    public class DefaultLogCleanerBuilder : ICleanerBuilder
    {
        public ICleaner Build(TimeSpan? retentionTime = null)
        {
            IReferenceTimeProvider referenceTimeProvider = new TodayMidnightReferenceTimeProvider();

            IFileDeletePolicy filePolicy = new FileLastWriteTimeDeletePolicy(retentionTime, referenceTimeProvider);
            IFolderDeletePolicy folderPolicy = new EmptyFolderCreationTimeDeletePolicy(retentionTime, referenceTimeProvider);

            return new FileSystemCleaner(filePolicy, folderPolicy);
        }
    }
}
