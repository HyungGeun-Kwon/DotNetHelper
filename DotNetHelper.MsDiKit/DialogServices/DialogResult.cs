using DotNetHelper.MsDiKit.Common;

namespace DotNetHelper.MsDiKit.DialogServices
{
    public class DialogResult : IDialogResult
    {
        public Parameters? Parameters { get; }
        public ButtonResult ButtonResult { get; }

        public DialogResult() { }
        public DialogResult(ButtonResult buttonResult) => ButtonResult = buttonResult;
        public DialogResult(ButtonResult buttonResult, Parameters? parameters)
        {
            ButtonResult = buttonResult;
            Parameters = parameters;
        }
    }
}
