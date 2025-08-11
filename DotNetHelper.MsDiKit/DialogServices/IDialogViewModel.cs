namespace DotNetHelper.MsDiKit.DialogServices
{
    public interface IDialogViewModel
    {
        void OnDialogOpened(DialogParameters? parameters);
        void OnDialogClosed();
    }
}
