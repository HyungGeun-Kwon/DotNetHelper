using System.Windows;

namespace DotNetHelper.MsDiKit.DialogServices
{
    public interface IDialogService
    {
        void AddDialog<TWindow, TDialogViewModel>(string viewKey = "")
            where TWindow : FrameworkElement
            where TDialogViewModel : class, IDialogViewModel;

        void Show(string viewKey, DialogParameters? parameters = null, Action? closedCallback = null);
        bool? ShowDialog(string viewKey, DialogParameters? parameters = null, Action? closedCallback = null);
    }
}
