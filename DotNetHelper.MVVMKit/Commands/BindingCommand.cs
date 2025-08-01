using System.Windows.Input;

namespace DotNetHelper.MVVMKit.Commands
{
    public sealed class BindingCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }

        public BindingCommand(Action execute, Func<bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke() ?? true;
        public void Execute(object? parameter) => _execute();

        public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }

    public class BindingCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T, bool>? _canExecute;

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public BindingCommand(Action<T> execute, Func<T, bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute, nameof(execute));
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter)
        {
            if (parameter is null)
            {
                if (typeof(T).IsValueType) return false;
                return _canExecute?.Invoke((T)parameter!) ?? true;
            }

            if (parameter is T t)
                return _canExecute?.Invoke(t) ?? true;

            return false;
        }

        public void Execute(object? parameter)
        {
            if (parameter is null && typeof(T).IsValueType)
                throw new InvalidCastException("Cannot pass null to value‑type command parameter.");

            if (parameter is not T t)
                throw new InvalidCastException($"Invalid command parameter type. Expected {typeof(T)}, got {parameter?.GetType()}.");

            _execute(t);
        }

        public static void RaiseCanExecuteChanged() => CommandManager.InvalidateRequerySuggested();
    }
}
