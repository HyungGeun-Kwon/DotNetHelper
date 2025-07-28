using DotNetHelper.Cleaner.Interfaces;

namespace DotNetHelper.Cleaner.Services
{
    /// <summary>
    /// 아무 것도 하지 않는 Cleaner (삭제 비활성화용 Null Object)
    /// </summary>
    public sealed class NoOpCleaner : ICleaner
    {
        public NoOpCleaner() { } 
        public void Cleanup(string _) { }
    }
}
