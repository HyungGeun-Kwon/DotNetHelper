namespace DotNetHelper.Cleaner.Interfaces
{
    public interface IReferenceTimeProvider
    {
        DateTime GetReferenceTime();
        DateTime GetReferenceTime(DateTime nowDateTime);
    }
}
