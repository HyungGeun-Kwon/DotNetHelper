using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetHelper.MsDiKit.DialogServices
{
    public class DialogService(IServiceProvider rootSp) : IDialogService
    {
        private readonly ConcurrentDictionary<string, (Type viewType, Type? viewModelType)> _map = [];
        private readonly IServiceProvider _rootSp = rootSp;

        /// <summary>
        /// View, ViewModel은 반드시 Scoped or Transient 형식으로 등록할것.
        /// View, ViewModel은 service에 따로 등록해줘야함.
        /// </summary>
        public void AddDialog<TView, TDialogViewModel>(string viewKey = "")
            where TView : FrameworkElement
            where TDialogViewModel : class, IDialogViewModel
        {
            viewKey = string.IsNullOrEmpty(viewKey) ? typeof(TView).Name : viewKey;

            if (!_map.TryAdd(viewKey, (typeof(TView), typeof(TDialogViewModel))))
                throw new InvalidOperationException($"Duplicate dialog key: '{viewKey}'");
        }

        public void Show(string viewKey, DialogParameters? parameters = null, Action? closedCallback = null)
        {
            RunOnUiThread(() =>
            {
                var (scope, window) = CreateWindowWithScope(viewKey);
                try
                {
                    AttachLifecycle(window, scope, parameters, closedCallback);
                    SetOwnerIfNull(window);
                    window.Show();
                }
                catch
                {
                    try { scope.Dispose(); } catch { }
                    throw;
                }
            });
        }
        public bool? ShowDialog(string viewKey, DialogParameters? parameters = null, Action? closedCallback = null)
        {
            bool? result = null;
            RunOnUiThread(() =>
            {
                var (scope, window) = CreateWindowWithScope(viewKey);
                try
                {
                    AttachLifecycle(window, scope, parameters, closedCallback);
                    SetOwnerIfNull(window, useMainWindowAsOwner: true);
                    result = window.ShowDialog();
                }
                catch
                {
                    try { scope.Dispose(); } catch { }
                    throw;
                }
            });
            return result;
        }
        private (IServiceScope scope, Window window) CreateWindowWithScope(string viewKey)
        {
            if (!_map.TryGetValue(viewKey, out (Type viewType, Type? viewModelType) entry))
                throw new KeyNotFoundException($"No dialog registered for key '{viewKey}'.");

            var scope = _rootSp.CreateScope();
            var sp = scope.ServiceProvider;

            // keyed 우선 -> 일반 폴백
            var ksp = sp as IKeyedServiceProvider;

            object? viewObj =
                ksp?.GetKeyedService(entry.viewType, viewKey) ??
                sp.GetService(entry.viewType);

            if (viewObj is null)
                throw new InvalidOperationException($"View type '{entry.viewType.FullName}' not found (keyed '{viewKey}' or unkeyed).");

            var window = viewObj switch
            {
                Window w => w,
                FrameworkElement fe => new Window
                {
                    Content = fe,
                    Width = fe.Width > 0 ? fe.Width : 400,
                    Height = fe.Height > 0 ? fe.Height : 300,
                    Title = viewKey,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                },
                _ => throw new InvalidOperationException(
                    $"Resolved view '{viewKey}' is not a FrameworkElement. Actual: {viewObj.GetType().FullName}")
            };

            // VM 해소 + 바인딩 (DataContext가 비어 있을 때만 적용)
            if (entry.viewModelType is not null && window.DataContext is null)
            {
                var vm = sp.GetRequiredService(entry.viewModelType);
                window.DataContext = vm;
            }

            return (scope, window);
        }
        private static void AttachLifecycle(Window window, IServiceScope scope, DialogParameters? parameters, Action? closedCallback)
        {
            void LoadedHandler(object? _, EventArgs __)
            {
                window.Loaded -= LoadedHandler;
                if (window.DataContext is IDialogViewModel dvm)
                    dvm.OnDialogOpened(parameters);
            }

            void ClosedHandler(object? s, EventArgs e)
            {
                window.Closed -= ClosedHandler;
                try
                {
                    if (window.DataContext is IDialogViewModel dvm2)
                        dvm2.OnDialogClosed();
                }
                finally
                {
                    try { scope.Dispose(); } catch { }
                    closedCallback?.Invoke();
                }
            }

            window.Loaded += LoadedHandler;
            window.Closed += ClosedHandler;
        }
        private static void RunOnUiThread(Action action)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null) { action(); return; }
            if (dispatcher.CheckAccess()) action(); else dispatcher.Invoke(action);
        }
        private static void SetOwnerIfNull(Window win, bool useMainWindowAsOwner = false)
        {
            if (win.Owner != null) return;

            if (useMainWindowAsOwner && Application.Current?.MainWindow != null)
            {
                win.Owner = Application.Current.MainWindow!;
                return;
            }

            var active = Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive);
            if (active != null && !ReferenceEquals(active, win))
                win.Owner = active;
        }
    }
}
