using DotNetHelper.MsDiKit.Common;

namespace DotNetHelper.MsDiKit.DialogServices
{
    public interface IDialogViewModel
    {
        event Action<IDialogResult>? RequestClose;
        void OnDialogOpened(Parameters? parameters);
        void OnDialogClosed();
    }
}
