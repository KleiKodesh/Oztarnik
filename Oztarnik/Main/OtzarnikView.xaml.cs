using Microsoft.VisualBasic;
using Ookii.Dialogs.WinForms;
using Otzarnik.FsViewer;
using Oztarnik.FileViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Xml.Serialization;

namespace Oztarnik.Main
{
    public partial class OtzarnikView : UserControl
    {
        public OtzarnikView()
        {
            InitializeComponent();
            this.Loaded += (s,_) => OpenRecentFiles();
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedIndex == 0)
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    FsSearchBox.Focus();
                    Keyboard.Focus(FsSearchBox);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);           
        }

        private void UserControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control && e.Key == Key.O)
            {
                MainTabControl.SelectedIndex = 0;
                e.Handled = true;
            }
                
        }

        private void FsTab_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    fsViewer.GoBack();
                    e.Handled = true;
                    break;
                case Key.Home:
                    fsViewer.Reset();
                    e.Handled = true;
                    break;
                case Key.Down:
                    fsViewer.SelectedIndex = fsViewer.SelectedIndex < fsViewer.Items.Count - 1 ?
                        fsViewer.SelectedIndex + 1 : 0;
                    fsViewer.Focus();
                    e.Handled = true;
                    break;
                case Key.Up:
                    fsViewer.SelectedIndex = fsViewer.SelectedIndex > 0 ?
                        fsViewer.SelectedIndex - 1 : fsViewer.Items.Count - 1;
                    fsViewer.Focus();
                    e.Handled = true;
                    break;
            }
        }

        private void fsViewer_NavigationRequested(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeItem treeItem)
                OpenFile(treeItem);
        }

        void OpenFile(TreeItem treeItem)
        {
            if (treeItem.Extension.ToLower().Contains("pdf"))
            {
                var webview = new WebViewLib.WebViewHost();
                FileViewerTabControl.Items.Add(new TabItem
                {
                    Header = treeItem.Name,
                    Content = webview,
                    IsSelected = true
                });
                webview.Navigate(treeItem.Path);
            }
            else
                FileViewerTabControl.Items.Add(new TabItem
                {
                    Header = treeItem.Name,
                    Content = new FileViewer.FileView(treeItem),
                    IsSelected = true
                });
            MainTabControl.SelectedIndex = 1;
        }

        private void FileViewerTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl)
            {
                if (tabControl.Items.Count == 0)
                {
                    MainTabControl.SelectedIndex = 0;
                }
                else
                {
                    List<string> openFiles = new List<string>();
                    foreach (TabItem item in tabControl.Items)
                        if (item.Content is FileView fileView)
                            openFiles.Add(fileView.TreeItem.Path);
                    Interaction.SaveSetting(AppDomain.CurrentDomain.BaseDirectory, "FileViewerTabs", "OpenFiles", JsonSerializer.Serialize(openFiles));
                }
            }
        }

        void OpenRecentFiles()
        {
            string json = Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "FileViewerTabs", "OpenFiles");
            if (!string.IsNullOrEmpty(json))
            {
                var list = JsonSerializer.Deserialize<List<string>>(json);
                var items = fsViewer.Root.EnumerateItems().Where(item => list.Contains(item.Path));
                foreach (var item in items)
                    OpenFile(item);
            }
        }
    }
}
