using System.ComponentModel;
using System.Reflection;

namespace DotNetHelper.Logger.Enums
{
    internal static class EnumExtensions
    {
        internal static string GetDescription(this Enum value)
        {
            return value.GetType()
                        .GetField(value.ToString())?
                        .GetCustomAttribute<DescriptionAttribute>()?
                        .Description ?? value.ToString();
        }
    }
}
