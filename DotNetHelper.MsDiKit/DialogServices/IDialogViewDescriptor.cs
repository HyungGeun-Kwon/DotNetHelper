namespace DotNetHelper.MsDiKit.DialogServices
{
    public interface IDialogViewDescriptor { string Key { get; } Type View { get; } Type? ViewModel { get; } }
}
