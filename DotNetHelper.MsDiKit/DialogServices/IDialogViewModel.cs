﻿using DotNetHelper.MsDiKit.Common;

namespace DotNetHelper.MsDiKit.DialogServices
{
    public interface IDialogViewModel
    {
        void OnDialogOpened(Parameters? parameters);
        void OnDialogClosed();
    }
}
