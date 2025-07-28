namespace DotNetHelper.Cleaner.Interfaces
{
    public interface IFolderDeletePolicy
    {
        bool ShouldDeleteFolder(string folderPath);
    }
}
