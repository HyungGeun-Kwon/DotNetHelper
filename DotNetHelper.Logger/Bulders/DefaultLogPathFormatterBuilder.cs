using System.Reflection;
using DotNetHelper.Logger.Interfaces;
using DotNetHelper.Logger.Services.LogFormatters;

namespace DotNetHelper.Logger.Bulders
{
    public class DefaultLogPathFormatterBuilder : ILogPathFormatterBuilder
    {
        // 내문서\프로젝트명\LogFiles\yyyy\MM\dd\yyyy-MM-dd.log
        public ILogPathFormatter Build()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            ArgumentNullException.ThrowIfNull(entryAssembly);

            var assemblyName = entryAssembly.GetName();
            ArgumentNullException.ThrowIfNull(entryAssembly);
            ArgumentNullException.ThrowIfNull(assemblyName.Name);

            return new LogPathFormatter(
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    assemblyName.Name,
                    "LogFiles",
                    $"{{{LogFormatter.DateTime}:yyyy}}",
                    $"{{{LogFormatter.DateTime}:MM}}",
                    $"{{{LogFormatter.DateTime}:dd}}",
                    $"{{{LogFormatter.DateTime}:yyyy-MM-dd}}.txt"));
        }
    }
}
