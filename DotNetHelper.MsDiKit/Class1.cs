using System.Collections.Concurrent;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetHelper.MsDiKit
{
    public interface IDialogViewModel
    {
        void OnDialogOpened(DialogParameters? parameters);
        void OnDialogClosed();
    }

    public class DialogParameters
    {
        private readonly Dictionary<string, object> _parameters = [];

        public void Add(string key, object value) => _parameters[key] = value;
        public bool ContainsKey(string key) => _parameters.ContainsKey(key);

        public object this[string key] => _parameters[key];

        public T GetValue<T>(string key)
        {
            if (!_parameters.TryGetValue(key, out object? value))
                throw new KeyNotFoundException($"Parameter with key '{key}' was not found.");
            
            if (value is not T tValue)
                throw new InvalidCastException($"Cannot convert parameter '{key}' to {typeof(T).Name}");

            return tValue;
        }

        public object GetValue(string key)
        {
            if (!_parameters.TryGetValue(key, out object? value))
                throw new KeyNotFoundException($"Parameter with key '{key}' was not found.");
            
            return value;
        }

        public bool TryGetValue<T>(string key, out T? value)
        {
            if (_parameters.TryGetValue(key, out var v) && v is T t) { value = t; return true; }
            value = default; return false;
        }

        public bool TryGetValue(string key, out object? value)
        {
            if (_parameters.TryGetValue(key, out value)) return true;
            value = default; return false;
        }
    }
    public interface IDialogService
    {
        void AddDialog<TWindow, TDialogViewModel>(string viewKey = "")
            where TWindow : FrameworkElement
            where TDialogViewModel : class, IDialogViewModel;

        void Show(string viewKey, DialogParameters? parameters = null, Action? closedCallback = null);
        bool ShowDialog(string viewKey, DialogParameters? parameters = null, Action? closedCallback = null);
    }
    public class DialogService : IDialogService
    {
        private readonly ConcurrentDictionary<string, (Type viewType, Type? viewModelType)> _map = [];
        private readonly IServiceProvider _sp;

        public DialogService(IServiceProvider sp)
        {
            _sp = sp;
        }

        public void AddDialog<TWindow, TDialogViewModel>(string viewKey = "")
            where TWindow : FrameworkElement
            where TDialogViewModel : class, IDialogViewModel
        {
            viewKey = string.IsNullOrEmpty(viewKey) ? typeof(TWindow).Name : viewKey;

            if (!_map.TryAdd(viewKey, (typeof(TWindow), typeof(TDialogViewModel))))
                throw new InvalidOperationException($"Duplicate dialog key: '{viewKey}'");
        }

        public void Show(string viewKey, DialogParameters? parameters = null, Action? closedCallback = null)
        {
            RunOnUiThread(() =>
            {
                var (scope, window) = CreateWindowWithScope(viewKey);
                AttachLifecycle(window, scope, parameters, closedCallback);
                SetOwnerIfNull(window);
                window.Show();
            });
            var window = CreateWindow(viewKey);
            window.Show();
            DialogViewDataContextHandler(window, parameters, closedCallback);
        }
        public bool ShowDialog(string viewKey, DialogParameters? parameters = null, Action? closedCallback = null)
        {
            var window = CreateWindow(viewKey);
            DialogViewDataContextHandler(window, parameters, closedCallback);
            bool? result = window.ShowDialog();
            return result ?? false;
        }
        private static void RunOnUiThread(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null) { action(); return; }
            if (dispatcher.CheckAccess()) action(); else dispatcher.Invoke(action);
        }
        private void DialogViewDataContextHandler(Window window, DialogParameters? parameters, Action? closedCallback)
        {
            if (window.DataContext is IDialogViewModel dvm)
            {
                dvm.OnDialogOpened(parameters);
                AttachCloseHandler(window, dvm, closedCallback);
            }
        }
        private void AttachCloseHandler(Window window, IDialogViewModel dvm, Action? closedCallback)
        {
            void Handler(object? _, EventArgs __)
            {
                window.Closed -= Handler;
                dvm.OnDialogClosed();
                closedCallback?.Invoke();
            }

            window.Closed += Handler;
        }
        private Window CreateWindow(string viewKey)
        {
            if (!_windows.ContainsKey(viewKey))
                throw new Exception(); // 적적한 Exception 날리기

            // ViewModel 자동 연결 Off로 View Resolve
            // ViewModel이 IDialogAware인 경우 parameters를 전달해주기 위함
            (Type viewType, Type? vmType) = _windows[viewKey];

            var view = _sp.GetRequiredService(viewType);

            Window window = view switch
            {
                Window w => w,
                FrameworkElement fe => new Window // UserControl, Page 등
                {
                    Content = fe,
                    Width = fe.Width > 0 ? fe.Width : 400,
                    Height = fe.Height > 0 ? fe.Height : 300,
                    Title = viewKey,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                },
                _ => throw new InvalidOperationException($"Resolved view '{viewKey}' is not a FrameworkElement. Actual type: {view.GetType().FullName}")
            };

            // viewModel이 등록되어있다면 DataContext 연결
            if (vmType is not null)
            {
                object? vm = _sp.GetRequiredService(vmType);

                window.DataContext = vm;
            }

            return window;
        }
    }
}
