using System.Windows;
using DotNetHelper.MVVMKit.Dialogs;
using DotNetHelper.MVVMKit.MVVM;
using DotNetHelper.MVVMKit.Regions;

namespace DotNetHelper.MVVMKit.DI
{
    public sealed partial class Container
    {
        /// <summary>
        /// 지정한 View를 Navigation용으로 Transient 방식 등록
        /// key 기본값 = 클래스명 (typeof(TView).Name)
        /// </summary>
        public void RegisterForNavigation<TView>(string? key = null) where TView : FrameworkElement
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            var viewType = typeof(TView);
            key ??= viewType.Name;
            _navigationViews.AddOrUpdate(key, viewType, (_, _) => viewType);
            RegisterTransient<TView>();
        }

        /// <summary>
        /// 지정한 View를 Navigation용으로 등록하고 ViewModelLocator에게 View와 ViewModel을 Transient 방식으로 등록함
        /// key 기본값 = 클래스명 (typeof(TView).Name)
        /// </summary>
        public void RegisterForNavigation<TView, TViewModel>(string? key = null)
            where TView : FrameworkElement
            where TViewModel : INavigationAware
        {
            RegisterForNavigation<TView>(key);
            ViewModelLocator.WireViewViewModel<TView, TViewModel>();
        }

        /// <summary>
        /// 지정한 View를 Dialog용으로 Transient 방식 등록
        /// key 기본값 = 클래스명 (typeof(TView).Name)
        /// </summary>
        public void RegisterDialog<TView>(string? key = null) where TView : FrameworkElement
        {
            var viewType = typeof(TView);
            key ??= viewType.Name;
            _dialogViews.AddOrUpdate(key, viewType, (_, _) => viewType);
            RegisterTransient<TView>();
        }

        /// <summary>
        /// 지정한 View를 Dialog용으로 등록하고 ViewModelLocator에게 View와 ViewModel을 Transient 방식으로 등록함
        /// 만약 ViewModel을 SingleTon으로 사용을 희망한다면 RegisterSingleton()으로 따로 등록
        /// key 기본값 = 클래스명 (typeof(TView).Name)
        /// </summary>
        public void RegisterDialog<TView, TViewModel>(string? key = null)
            where TView : FrameworkElement
            where TViewModel : class, IDialogAware
        {
            RegisterDialog<TView>(key);
            ViewModelLocator.WireViewViewModel<TView, TViewModel>();
        }

        internal Type GetNavigationViewType(string key)
        {
            if (_navigationViews.TryGetValue(key, out var navType)) return navType;
            throw new InvalidOperationException($"Navigation view with key '{key}' is not registered.");
        }

        internal Type GetDialogViewType(string key)
        {
            if (_dialogViews.TryGetValue(key, out var dlgType)) return dlgType;
            throw new InvalidOperationException($"Dialog view with key '{key}' is not registered.");
        }

        /// <summary>
        /// View와 ViewModel을 함께 Resolve (튜플로 반환)
        /// View - ViewModel이 ViewModelLocator에 정의되어있지 않으면 ViewModel을 연결하지 않고 viewModel을 null로 반환
        /// </summary>
        public (TFrameworkElement frameworkElement, object? viewModel) ResolveWithViewModel<TFrameworkElement>()
            where TFrameworkElement : FrameworkElement
        {
            var (fe, vm) = ResolveWithViewModel(typeof(TFrameworkElement));
            return ((TFrameworkElement)fe, vm);
        }

        /// <summary>
        /// type은 반드시 FrameworkElement여야함.
        /// View와 ViewModel을 함께 Resolve (튜플로 반환)
        /// View - ViewModel이 ViewModelLocator에 정의되어있지 않으면 ViewModel을 연결하지 않고 viewModel을 null로 반환
        /// </summary>
        public (FrameworkElement frameworkElement, object? viewModel) ResolveWithViewModel(Type viewType)
        {
            ObjectDisposedException.ThrowIf(IsDisposed, this);
            if (!typeof(FrameworkElement).IsAssignableFrom(viewType))
                throw new InvalidOperationException($"Type '{viewType.FullName}' must inherit from FrameworkElement.");

            var resolved = Resolve(viewType);

            if (resolved is not FrameworkElement fe)
                throw new InvalidOperationException($"Resolved instance is not a FrameworkElement: {resolved.GetType().FullName}");

            object? vm = null;

            if (ViewModelLocator.IsMapping(viewType))
            {
                Type vmType = ViewModelLocator.GetViewModelTypeForView(viewType);
                vm = Resolve(vmType);
                fe.DataContext = vm;
            }

            return (fe, vm);
        }

        /// <summary>
        /// View만 Resolve하고 ViewModel을 자동 연결 (DataContext에만 주입)
        /// View - ViewModel이 ViewModelLocator에 정의되어있지 않으면 ViewModel을 연결하지 않음
        /// </summary>
        public TFrameworkElement ResolveFrameworkElement<TFrameworkElement>()
            where TFrameworkElement : FrameworkElement
                    => (TFrameworkElement)ResolveFrameworkElement(typeof(TFrameworkElement));

        /// <summary>
        /// View만 Resolve하고 ViewModel을 자동 연결 (DataContext에만 주입)
        /// View - ViewModel이 ViewModelLocator에 정의되어있지 않으면 ViewModel을 연결하지 않음
        /// </summary>
        public FrameworkElement ResolveFrameworkElement(Type viewType)
        {
            var (fe, _) = ResolveWithViewModel(viewType);
            return fe;
        }
    }
}
