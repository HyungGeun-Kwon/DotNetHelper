using System;

namespace DotNetHelper.Cleaner.Interfaces
{
    public interface ICleanerBuilder
    {
        ICleaner Build(TimeSpan? retentionTime = null);
    }
}
