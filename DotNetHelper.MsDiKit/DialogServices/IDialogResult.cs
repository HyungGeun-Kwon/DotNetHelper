using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetHelper.MsDiKit.Common;

namespace DotNetHelper.MsDiKit.DialogServices
{
    public interface IDialogResult
    {
        Parameters? Parameters { get; }
        ButtonResult ButtonResult { get; }
    }
}
