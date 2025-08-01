using DotNetHelper.Cleaner.Interfaces;

namespace DotNetHelper.Cleaner.Provider
{
    public class NowReferenceTimeProvider : IReferenceTimeProvider
    {
        public DateTime GetReferenceTime() => DateTime.Now;

        public DateTime GetReferenceTime(DateTime nowDateTime) => nowDateTime;
    }
}
