using DotNetHelper.Cleaner.Interfaces;

namespace DotNetHelper.Cleaner.Provider
{
    public class TodayMidnightReferenceTimeProvider : IReferenceTimeProvider
    {
        public DateTime GetReferenceTime() => DateTime.Today;

        public DateTime GetReferenceTime(DateTime nowDateTime) => nowDateTime.Date;
    }
}
