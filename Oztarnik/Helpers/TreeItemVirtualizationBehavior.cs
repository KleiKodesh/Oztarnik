using System.Windows.Controls;
using System.Windows;
using Otzarnik.FsViewer;
using System.Collections.Generic;

namespace Oztarnik.Helpers
{
    public static class TreeItemVirtualizationBehavior
    {
        public static bool GetEnableVirtualization(DependencyObject obj) =>
            (bool)obj.GetValue(EnableVirtualizationProperty);

        public static void SetEnableVirtualization(DependencyObject obj, bool value) =>
            obj.SetValue(EnableVirtualizationProperty, value);

        public static readonly DependencyProperty EnableVirtualizationProperty =
            DependencyProperty.RegisterAttached("EnableVirtualization", typeof(bool), typeof(TreeItemVirtualizationBehavior),
                new PropertyMetadata(false, OnEnableVirtualizationChanged));

        private static void OnEnableVirtualizationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TreeViewItem item && e.NewValue is bool enabled)
            {
                if (enabled)
                {
                    item.Expanded += OnItemExpanded;
                    item.Collapsed += OnItemCollapsed;
                    item.Loaded += Item_Loaded;
                }
                else
                {
                    item.Expanded -= OnItemExpanded;
                    item.Collapsed -= OnItemCollapsed;
                    item.Loaded -= Item_Loaded;
                }
            }
        }

        private static void Item_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem &&
                treeViewItem.DataContext is TreeItem treeItem &&
                treeItem.Items?.Count > 0)
                    treeViewItem.ItemsSource = new List<string> { "" };
        }

        private static void OnItemExpanded(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem &&
                 treeViewItem.DataContext is TreeItem treeItem &&
                 treeItem.Items?.Count > 0)
                    treeViewItem.ItemsSource = treeItem.Items;
        }

        private static void OnItemCollapsed(object sender, RoutedEventArgs e)
        {
            if (sender is TreeViewItem treeViewItem &&
                 treeViewItem.DataContext is TreeItem treeItem && 
                 treeItem.Items?.Count > 0)
                    treeViewItem.ItemsSource = new List<string> { ""};
        }
    }

}
