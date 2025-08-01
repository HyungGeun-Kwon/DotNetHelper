namespace DotNetHelper.MVVMKit.Dialogs
{
    public interface IDialogAware
    {
        void OnDialogOpened(DialogParameters parameters);
        void OnDialogClosed();
    }
}
