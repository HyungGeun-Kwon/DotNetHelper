using System.ComponentModel;

namespace DotNetHelper.Logger.Enums
{
    public enum ELogKeyword
    {
        [Description("DEF")]
        Default,
        [Description("SEQ")]
        Sequence,
        [Description("BOT")]
        Bootstrap,
        [Description("SET")]
        Setting,
        [Description("VIW")]
        View,
    }
}
