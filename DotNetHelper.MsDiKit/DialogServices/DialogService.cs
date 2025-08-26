using System.Collections.Concurrent;
using System.Windows;
using DotNetHelper.MsDiKit.Common;
using DotNetHelper.MsDiKit.RegionServices;
using Microsoft.Extensions.DependencyInjection;

namespace DotNetHelper.MsDiKit.DialogServices
{
    public class DialogService : IDialogService
    {
        private readonly ConcurrentDictionary<string, (Type viewType, Type? viewModelType)> _map = [];
        private readonly IServiceProvider _rootSp;

        public DialogService(IServiceProvider rootSp, IEnumerable<IDialogViewDescriptor> descriptors)
        {
            _rootSp = rootSp;
            foreach (var d in descriptors)
            {
                if (!_map.TryAdd(d.Key, (d.View, d.ViewModel)))
                    throw new InvalidOperationException($"Duplicate region key '{d.Key}' from {d.View.FullName}.");
            }
        }

        public void AddDialog<TView, TDialogViewModel>(string viewKey = "")
            where TView : FrameworkElement
            where TDialogViewModel : class, IDialogViewModel
            => AddDialog(GetViewKey<TView>(viewKey), typeof(TView), typeof(TDialogViewModel));

        public void AddDialog<TView>(string viewKey = "")
            where TView : FrameworkElement
            => AddDialog(GetViewKey<TView>(viewKey), typeof(TView));

        private void AddDialog(string viewKey, Type view, Type? viewModel = null)
        {
            if (!_map.TryAdd(viewKey, (view, viewModel)))
                throw new InvalidOperationException($"Duplicate dialog key: '{viewKey}'");
        }

        public void AddOrUpdateDialog<TView, TDialogViewModel>(string viewKey = "")
            where TView : FrameworkElement
            where TDialogViewModel : class, IDialogViewModel
            => AddOrUpdateDialog(GetViewKey<TView>(viewKey), typeof(TView), typeof(TDialogViewModel));
        
        public void AddOrUpdateDialog<TView>(string viewKey = "")
            where TView : FrameworkElement
            => AddOrUpdateDialog(GetViewKey<TView>(viewKey), typeof(TView));
        
        private void AddOrUpdateDialog(string viewKey, Type view, Type? viewModel = null)
            => _map.AddOrUpdate(viewKey, (view, viewModel), (_, _) => (view, viewModel));


        public bool TryRemoveDialog(string viewKey) => _map.TryRemove(viewKey, out _);
        public bool TryRemoveDialog<TView>() where TView : FrameworkElement => TryRemoveDialog(typeof(TView).Name);


        private string GetViewKey<TView>(string viewKey) => string.IsNullOrEmpty(viewKey) ? typeof(TView).Name : viewKey;


        public void Show(string viewKey, Parameters? parameters = null, Action? closedCallback = null)
        {
            RunOnUiThread(() =>
            {
                IServiceScope scope = _rootSp.CreateScope();
                try
                {
                    var window = CreateWindowWithScope(viewKey, scope);
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
        public bool? ShowDialog(string viewKey, Parameters? parameters = null, Action? closedCallback = null)
        {
            bool? result = null;
            RunOnUiThread(() =>
            {
                IServiceScope scope = _rootSp.CreateScope();
                try
                {
                    var window = CreateWindowWithScope(viewKey, scope);
                    AttachLifecycle(window, scope, parameters, closedCallback);
                    SetOwnerIfNull(window);
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
        private Window CreateWindowWithScope(string viewKey, IServiceScope scope)
        {
            if (!_map.TryGetValue(viewKey, out (Type viewType, Type? viewModelType) entry))
                throw new KeyNotFoundException($"No dialog registered for key '{viewKey}'.");
           
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
                    SizeToContent = SizeToContent.WidthAndHeight,
                    Width = fe.Width > 0 ? fe.Width : Double.NaN,
                    Height = fe.Height > 0 ? fe.Height : Double.NaN,
                    Title = string.IsNullOrWhiteSpace(fe.Name) ? viewKey : fe.Name,
                    WindowStartupLocation = WindowStartupLocation.CenterOwner
                },
                _ => throw new InvalidOperationException($"Resolved view '{viewKey}' is not a FrameworkElement. Actual: {viewObj.GetType().FullName}")
            };

            // VM 해소 + 바인딩 (DataContext가 비어 있을 때만 적용)
            if (entry.viewModelType is not null && window.DataContext is null)
            {
                var vm =
                    ksp?.GetKeyedService(entry.viewModelType, viewKey) ??
                    sp.GetService(entry.viewModelType);

                if (vm is null)
                    throw new InvalidOperationException(
                        $"ViewModel '{entry.viewModelType.FullName}' not found (keyed '{viewKey}' or unkeyed).");

                window.DataContext = vm;
            }

            return window;
        }
        private static void AttachLifecycle(Window window, IServiceScope scope, Parameters? parameters, Action? closedCallback)
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
        private static void SetOwnerIfNull(Window win)
        {
            if (win.Owner != null) return;

            Window? owner =
                Application.Current?.Windows.OfType<Window>().FirstOrDefault(w => w.IsActive && w.IsVisible)
                ?? Application.Current?.MainWindow;

            if (owner != null && !ReferenceEquals(owner, win))
                win.Owner = owner;
        }
    }
}
