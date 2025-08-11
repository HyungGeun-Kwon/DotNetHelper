using System.Windows;
using DotNetHelper.MsDiKit.DialogServices;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotNetHelper.MsDiKit.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDialogService(this IServiceCollection services)
        {
            services.TryAddSingleton<IDialogService, DialogService>();
            return services;
        }

        public static IServiceCollection AddDialogView<TView, TViewModel>(this IServiceCollection services, string viewName = "")
            where TView : FrameworkElement
            where TViewModel : class, IDialogViewModel
        {
            if (string.IsNullOrEmpty(viewName)) services.AddTransient<TView>();
            else services.AddKeyedTransient<TView>(viewName);

            services.AddScoped<TViewModel>();
            return services;
        }
    }
}
