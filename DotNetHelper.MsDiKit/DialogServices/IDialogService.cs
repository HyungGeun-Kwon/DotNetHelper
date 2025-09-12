using System.Windows;
using DotNetHelper.MsDiKit.Common;

namespace DotNetHelper.MsDiKit.DialogServices
{
    /// <summary>
    /// View, ViewModel은 반드시 Scoped or Transient 형식으로 등록할것.
    /// View, ViewModel은 service에 따로 등록해줘야함.
    /// </summary>
    public interface IDialogService
    {
        void AddDialog<TView, TDialogViewModel>(string viewKey = "") where TView : FrameworkElement where TDialogViewModel : class, IDialogViewModel;
        void AddDialog<TView>(string viewKey = "") where TView : FrameworkElement;

        void AddOrUpdateDialog<TView, TDialogViewModel>(string viewKey = "") where TView : FrameworkElement where TDialogViewModel : class, IDialogViewModel;
        void AddOrUpdateDialog<TView>(string viewKey = "") where TView : FrameworkElement;

        bool TryRemoveDialog(string viewKey);
        bool TryRemoveDialog<TView>() where TView : FrameworkElement;

        void Show(string viewKey, Parameters? parameters = null, Action<IDialogResult?>? closedCallback = null);
        bool? ShowDialog(string viewKey, Parameters? parameters = null, Action<IDialogResult?>? closedCallback = null);
    }
}
