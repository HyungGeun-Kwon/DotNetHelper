using System.Windows;
using DotNetHelper.MsDiKit.DialogServices;
using DotNetHelper.MsDiKit.RegionServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetHelper.MsDiKit.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRegionService(this IServiceCollection services, Func<IServiceProvider> getServiceProvider)
        {
            services.TryAddSingleton<IRegionService, RegionService>();
            RegionServiceAttached.Configure(getServiceProvider);
            return services;
        }

        /// <summary>
        /// IServiceCollection에 TView는 AddTransient, TViewModel은 AddScoped로 등록
        /// viewName이 Null 또는 Empty가 아니라면 KeyedTransient로 등록.
        /// 여기에서 등록한 View와 ViewModel들을 IRegionService 생성 시점에 RegionService에도 등록해줌.
        /// </summary>
        public static IServiceCollection AddRegionView<TView, TViewModel>(this IServiceCollection services, string viewName = "")
            where TView : FrameworkElement
            where TViewModel : class, IRegionViewModel
        {
            if (string.IsNullOrEmpty(viewName)) services.AddTransient<TView>();
            else services.AddKeyedTransient<TView>(viewName);

            services.AddSingleton<IRegionViewDescriptor>(new RegionViewDescriptor(viewName, typeof(TView), typeof(TViewModel)));

            services.AddScoped<TViewModel>();
            return services;
        }

        public static IServiceCollection AddDialogService(this IServiceCollection services)
        {
            services.TryAddSingleton<IDialogService, DialogService>();


            return services;
        }

        /// <summary>
        /// IServiceCollection에 TView는 AddTransient, TViewModel은 AddScoped로 등록
        /// viewName이 Null 또는 Empty가 아니라면 KeyedTransient로 등록.
        /// 여기에서 등록한 View와 ViewModel들을 IDialogService 생성 시점에 DialogService에도 등록해줌.
        /// </summary>
        public static IServiceCollection AddDialogView<TView, TViewModel>(this IServiceCollection services, string viewName = "")
            where TView : FrameworkElement
            where TViewModel : class, IDialogViewModel
        {
            if (string.IsNullOrEmpty(viewName)) services.AddTransient<TView>();
            else services.AddKeyedTransient<TView>(viewName);

            services.AddSingleton<IDialogViewDescriptor>(new DialogViewDescriptor(viewName, typeof(TView), typeof(TViewModel)));

            services.AddScoped<TViewModel>();
            return services;
        }
    }
}
