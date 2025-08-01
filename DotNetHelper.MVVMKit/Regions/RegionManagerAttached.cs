﻿using System.Windows;
using System.Windows.Controls;
using DotNetHelper.MVVMKit.App;

namespace DotNetHelper.MVVMKit.Regions
{
    public static class RegionManagerAttached
    {
        public static readonly DependencyProperty RegionNameProperty =
            DependencyProperty.RegisterAttached(
                "RegionName",
                typeof(string),
                typeof(RegionManagerAttached),
                new PropertyMetadata(null, OnRegionNameChanged));

        public static void SetRegionName(DependencyObject element, string value)
        {
            element.SetValue(RegionNameProperty, value);
        }

        public static string GetRegionName(DependencyObject element)
        {
            return (string)element.GetValue(RegionNameProperty);
        }

        private static void OnRegionNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ContentControl control && e.NewValue is string regionName)
            {
                if (Application.Current is MVVMKitApplication app)
                {
                    var regionManager = (RegionManager)app.Container.Resolve<IRegionManager>();
                    regionManager.RegisterRegionName(regionName, control);
                }
            }
        }
    }
}
