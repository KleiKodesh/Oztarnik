using Microsoft.VisualBasic;
using Microsoft.Win32;
using Otzarnik.FsViewer;
using Oztarnik.Favorites;
using Oztarnik.FileViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Oztarnik.Main
{
    public partial class OtzarnikView : UserControl
    {
        public OtzarnikView()
        {
            InitializeComponent();
        }

        private void MainTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedIndex == -1)
            {
                MainTabControl.SelectedIndex = 0;
                return;
            }
                
            if (e.OriginalSource == MainTabControl && MainTabControl.SelectedIndex == 0)
            {
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    FsSearchBox.Focus();
                    Keyboard.Focus(FsSearchBox);
                }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
            }
        }

        private void FileViewerTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (MainTabControl.SelectedIndex == 1 && FileViewerTabControl.Items.Count == 0)
                MainTabControl.SelectedIndex = 0;
        }


        private void FileViewerTabControl_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
                switch (e.Key)
                {
                    case Key.O:
                        OpenFile();
                        e.Handled = true;
                        break;
                    case Key.W:
                        CloseCurrentTab();
                        e.Handled = true;
                        break;
                    case Key.X:
                        CloseAllTabs();
                        e.Handled = true;
                        break;
                    case Key.H:
                        ShowFavorites();
                        e.Handled = true;
                        break;
                }
        }

        public void ShowFavorites() =>
            MainTabControl.SelectedIndex = 3;

        public void OpenFile() =>
            MainTabControl.SelectedIndex = 0;

        public void CloseCurrentTab() =>
           FileViewerTabControl.Items.RemoveAt(FileViewerTabControl.SelectedIndex);

        public void CloseAllTabs()
        {
            var items = FileViewerTabControl.Items;
            foreach (TabItem tab in items)
                FileViewerTabControl.Items.Remove(tab);
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
                LoadFile(treeItem, "");
        }

        void LoadFile(TreeItem treeItem, string scrollIndex)
        {
            string extension = Path.GetExtension(treeItem.Path).ToLower();
            if (!Regex.IsMatch(extension, @"^\.(pdf|txt|html)$"))
                return;

            if (extension.Contains("pdf"))
            {
                var webview = new WebViewLib.WebViewHost();
                var tab = new TabItem
                {
                    Header = treeItem.Name?? Path.GetFileNameWithoutExtension(treeItem.Path),
                    Content = webview,
                    IsSelected = true
                };
                FileViewerTabControl.Items.Add(tab);
                webview.Navigate(treeItem.Path);
            }
            else
            {
                FileViewerTabControl.Items.Add(new TabItem
                {
                    Header = treeItem.Name ?? Path.GetFileNameWithoutExtension(treeItem.Path),
                    Content = new FileView(treeItem, scrollIndex),
                    IsSelected = true
                });
            }

            MainTabControl.SelectedIndex = 1;
            Dispatcher.BeginInvoke(new Action(() =>
            {
                FileViewerTabControl.Focus();
                Keyboard.Focus(FileViewerTabControl);
            }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
        }

        private void HistoryButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is HistoryItem bookMark)
            {
                var treeItem = fsViewer.Root.EnumerateItems().FirstOrDefault(item => item.Path == bookMark.Path);
                if (treeItem != null)
                    LoadFile(treeItem, "");
            }
        }

        private void BookMarkButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is BookMarkModel bookMark)
            {
                var treeItem = fsViewer.Root.EnumerateItems().FirstOrDefault(item => item.Path == bookMark.Path);
                if (treeItem != null)
                    LoadFile(treeItem, bookMark.ScrollIndex);
            }
        }

        private async void SaveEnviromentButton_Click(object sender, RoutedEventArgs e)
        {
            List<BookMarkModel> bookMarks = new List<BookMarkModel>();
            foreach (TabItem item in FileViewerTabControl.Items)
                if (item.Content is FileView fileView)
                {
                    var bookMark = await fileView.CreateBookMark();
                    bookMarks.Add(bookMark);
                }

            if (bookMarks.Count > 0)
                EnviromentsViewModel.AddEnviroment(bookMarks);
        }


        private void EnviromentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is EnviromentModel enviroment)
                foreach (var bookMark in enviroment.Bookmarks)
                {
                    TreeItem treeItem = fsViewer.Root.EnumerateItems().FirstOrDefault(item => item.Path == bookMark.Path);
                    if (treeItem != null)
                        LoadFile(treeItem, bookMark.ScrollIndex);
                }
        }

        private void ExternalFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog { Title = "בחר קובץ",  Filter = "*.html;*.txt;*.pdf|*.html;*.txt;*.pdf" };

            if (openFileDialog.ShowDialog() == true)
                LoadFile(new TreeItem { Path = openFileDialog.FileName }, "");
        }
    }
}
