namespace DotNetHelper.Cleaner.Interfaces
{
    public interface IFileDeletePolicy
    {
        bool ShouldDeleteFile(string filePath);
    }
}
