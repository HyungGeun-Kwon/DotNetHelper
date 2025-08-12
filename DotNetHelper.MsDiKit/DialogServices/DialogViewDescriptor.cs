namespace DotNetHelper.MsDiKit.DialogServices
{
    internal sealed record DialogViewDescriptor(string Key, Type View, Type? ViewModel) : IDialogViewDescriptor;
}
