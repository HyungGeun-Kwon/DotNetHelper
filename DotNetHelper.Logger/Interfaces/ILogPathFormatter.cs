namespace DotNetHelper.Logger.Interfaces
{
    public interface ILogPathFormatter : ILogFormatter
    {
        string RootPath { get; }
    }
}
