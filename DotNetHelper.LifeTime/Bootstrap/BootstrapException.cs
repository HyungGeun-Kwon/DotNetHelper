using System;

namespace DotNetHelper.LifeTime.Bootstrap
{
    public class BootstrapException : Exception
    {
        public string FailName { get; }

        public BootstrapException(string failName)
            : base($"Load {failName} Fail.")
        {
            FailName = failName;
        }

        public BootstrapException(string failName, Exception innerException)
            : base($"Load {failName} Fail.", innerException)
        {
            FailName = failName;
        }
    }
}
