using DotNetHelper.Cleaner.Interfaces;

namespace DotNetHelper.Cleaner.Provider
{
    public class TodayFixedHourReferenceTimeProvider : IReferenceTimeProvider
    {
        private readonly int _hour;
        public TodayFixedHourReferenceTimeProvider(int hour)
        {
            _hour = hour;
        }

        public DateTime GetReferenceTime() => DateTime.Today.AddHours(_hour);

        public DateTime GetReferenceTime(DateTime nowDateTime) => nowDateTime.Date.AddHours(_hour);
    }
}
