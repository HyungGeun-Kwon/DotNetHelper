using System.ComponentModel;

namespace DotNetHelper.Logger.Models
{
    public enum ELogLevel : int
    {
        [Description("VRB")]
        Verbose = 0,
        [Description("DBG")]
        Debug = 1,
        [Description("INF")]
        Info = 2,
        [Description("WRN")]
        Warning = 3,
        [Description("ERR")]
        Error = 4,
        [Description("FTL")]
        Fatal = 5
    }
}
