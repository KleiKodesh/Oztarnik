using Microsoft.Win32;
using Otzarnik.FsViewer;
using Otzarnik.Search;
using Oztarnik.AppData;
using Oztarnik.FileViewer;
using Oztarnik.FsViewer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            var items = FileViewerTabControl.Items.Cast<TabItem>().ToList();
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

        public void LoadFile(TreeItem treeItem, string scrollIndex, ResultModel resultModel = null)
        {
            try
            {
                if (!Regex.IsMatch(treeItem.Extension, @"^\.(pdf|txt|html)$"))
                    return;

                if (treeItem.Extension.Contains("pdf"))
                {
                    var webview = new WebViewLib.WebViewHost();
                    var tab = new TabItem
                    {
                        Header = treeItem.Name ?? Path.GetFileNameWithoutExtension(treeItem.Path),
                        Content = webview,
                        IsSelected = true
                    };

                    FileViewerTabControl.Items.Add(tab);
                    webview.Navigate(treeItem.Path);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        FileViewerTabControl.Focus();
                        Keyboard.Focus(FileViewerTabControl);
                    }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }
                else
                {
                    var fileView = resultModel == null ? new FileView(treeItem, scrollIndex) :
                        new FileView(resultModel, scrollIndex);

                    FileViewerTabControl.Items.Add(new TabItem
                    {
                        Header = treeItem.Name ?? Path.GetFileNameWithoutExtension(treeItem.Path),
                        Content = fileView,
                        IsSelected = true
                    });

                    string groupId = FsSort.FileOrder.FirstOrDefault(id => treeItem.Name.Contains(id)) ?? string.Empty;
                    fileView.RelativeBooksList.ItemsSource = fsViewer.Root.EnumerateItems()
                        .Where(item => item.Path != treeItem.Path && item.Name.Contains(groupId))
                        .OrderBy(t => t.Name);

                    if (!Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            FileViewerTabControl.Focus();
                            Keyboard.Focus(FileViewerTabControl);
                        }), System.Windows.Threading.DispatcherPriority.ApplicationIdle);
                }

                HistoryViewModel.AddHistoryItem(treeItem.Path);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                FileViewToggleButton.IsChecked = true;
            }
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

        private async void SaveEnvironmentButton_Click(object sender, RoutedEventArgs e)
        {
            List<BookMarkModel> bookMarks = new List<BookMarkModel>();
            foreach (TabItem item in FileViewerTabControl.Items)
                if (item.Content is FileView fileView)
                {
                    var bookMark = await fileView.CreateBookMark();
                    bookMarks.Add(bookMark);
                }

            if (bookMarks.Count > 0)
                EnvironmentsViewModel.AddEnvironment(bookMarks);
        }


        private void EnvironmentButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is EnvironmentModel Environment)
                foreach (var bookMark in Environment.Bookmarks)
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
                LoadFile(new TreeItem 
                {
                    Path = openFileDialog.FileName, 
                    Name = Path.GetFileNameWithoutExtension(openFileDialog.FileName) 
                }, "");
        }

        private void SearchResultsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is ListView listView && listView.SelectedItem is ResultModel resultModel)
            {
                LoadFile(resultModel.TreeItem, "", resultModel);
                listView.SelectedIndex = -1;
            }
        }
    }
}
