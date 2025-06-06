using System.Windows.Controls;
using System;
using Otzarnik.FsViewer;
using System.Windows.Input;
using System.Windows.Threading;
using Oztarnik.Main;
using Oztarnik.AppData;
using System.Threading.Tasks;
using WpfLib.Helpers;
using Otzarnik.FileViewer;
using System.Windows;
using Otzarnik.Helpers;
using Otzarnik.Search;

namespace Oztarnik.FileViewer
{
    public partial class FileView : UserControl
    {
        HeaderTreeItem _root;
        OtzarnikView _mainView;
        TabItem _parentTab;
        Window _parentWindow;
        
        public TreeItem TreeItem {get; set;}

        public FileView(TreeItem treeItem, string scrollIndex, string targetHeaderIndex)
        {
            this.Loaded += FileView_Loaded;
            this.TreeItem = treeItem;
            InitializeComponent();
            LoadFile(treeItem, scrollIndex, targetHeaderIndex);
        }

        public FileView(ResultModel result, string scrollIndex)
        {
            this.Loaded += FileView_Loaded;
            this.TreeItem = result.TreeItem;
            InitializeComponent();
            LoadResult(result, scrollIndex);
        }

        private void FileView_Loaded(object sender, RoutedEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
                FocusHeadersTextBox();
        }

        public void FocusHeadersTextBox()
        {
            HeadersPopup.IsOpen = true;

            //Dispatcher.BeginInvoke(new Action(() =>
            //{
            //    HeadersPopup.Focus();
            //    Keyboard.Focus(HeadersPopup);
            //}), DispatcherPriority.ApplicationIdle);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                NavigationTextBox.Focus();
                Keyboard.Focus(NavigationTextBox);
            }), DispatcherPriority.ApplicationIdle);
        }

        async void LoadResult(ResultModel result, string scrollIndex)
        {
            var contentModel = await ContentParser.Parse(result.TreeItem, true, result);

            _root = contentModel.RootHeader;
            headersListBox.Root = contentModel.RootHeader;
            viewer.LoadDocument(contentModel.Content, scrollIndex, true, "");
            NavigationTextBox.Focus();
        }

        async void LoadFile(TreeItem treeItem, string scrollIndex, string targetHeaderIndex)
        {
            var contentModel = await ContentParser.Parse(treeItem, true, null);
            _root = contentModel.RootHeader;
            headersListBox.Root = contentModel.RootHeader;           
            viewer.LoadDocument(contentModel.Content, scrollIndex, false, targetHeaderIndex);
            NavigationTextBox.Focus();  
        }

        private void headersListBox_NavigationRequested(object sender, System.Windows.RoutedEventArgs e)
        {
            if (e.OriginalSource is HeaderTreeItem item)
                viewer.NavigateToLine(item.LineIndex);
        }

        private void NavigationPanel_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Escape:
                    headersListBox.GoBack();
                    e.Handled = true;
                    break;
                case Key.Home:
                    headersListBox.Reset();
                    e.Handled = true;
                    break;
                case Key.Down:
                    headersListBox.SelectedIndex = headersListBox.SelectedIndex < headersListBox.Items.Count - 1 ?
                        headersListBox.SelectedIndex + 1 : 0;
                    headersListBox.Focus();
                    e.Handled = true;
                    break;
                case Key.Up:
                    headersListBox.SelectedIndex = headersListBox.SelectedIndex > 0 ?
                        headersListBox.SelectedIndex - 1 : headersListBox.Items.Count - 1;
                    headersListBox.Focus();
                    e.Handled = true;
                    break;
            }
        }

        private async void BookmarkButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            string scrollIndex = await viewer.GetScrollIndex();
            BookmarksViewModel.AddBookmark(TreeItem.Path, scrollIndex);
        }

        public async Task<BookMarkModel> CreateBookMark()
        {
            return new BookMarkModel
            {
                Path = TreeItem.Path,
                ScrollIndex = await viewer.GetScrollIndex(),
            };
        }

        private void HeadersPopup_Opened(object sender, EventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                NavigationTextBox.Focus();
                Keyboard.Focus(NavigationTextBox);
            }), DispatcherPriority.ApplicationIdle);
        }

        private void HeadersPopup_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                HeadersPopup.IsOpen = false;
        }

        private void RelativeBooksList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (RelativeBooksList.SelectedItem is TreeItem treeItem)
            {                
                DependencyHelper.FindParent<OtzarnikView>(this).LoadFile(treeItem, "");
                RelativeBooksPopup.IsOpen = false;
                RelativeBooksList.SelectedIndex = -1;
            }
        }

        private void OpenInNewWindow_Click(object sender, RoutedEventArgs e)
        {
            if (this.Parent is TabItem tabItem && tabItem.Parent is TabControl tabControl)
            {
                if (tabControl.Parent is Window window && _mainView != null)
                {
                    tabControl.Items.Remove(tabItem);
                    _mainView.FileViewerTabControl.Items.Add(tabItem);
                    _mainView.MainTabControl.SelectedIndex = 1;
                    window.Close();
                }
                else
                {
                    _mainView = DependencyHelper.FindParent<OtzarnikView>(tabControl);

                    var parentWindow = new FileViewerWindow();
                    var owner = DependencyHelper.FindParent<Window>(tabControl);
                    if (owner != null)
                        parentWindow.Owner = owner;

                    WdWpfWindowHelper.SetWordWindowOwner(parentWindow);

                    parentWindow.Title = tabItem.Header.ToString();
                    tabControl.Items.Remove(tabItem);
                    parentWindow.tabControl.Items.Add(tabItem);

                    parentWindow.Show();
                }
                tabItem.IsSelected = true;
            }
        }

        //private bool _waitingForMouseRelease = false;

        //private void Grid_MouseLeave(object sender, MouseEventArgs e)
        //{
        //    if (Mouse.LeftButton == MouseButtonState.Pressed)
        //    {
        //        Mouse.AddPreviewMouseUpHandler(Application.Current.MainWindow, GlobalMouseUpHandler);
        //    }
        //}

        //private void GlobalMouseUpHandler(object sender, MouseButtonEventArgs e)
        //{
        //        Mouse.RemovePreviewMouseUpHandler(Application.Current.MainWindow, GlobalMouseUpHandler);

        //        // Mouse was released after leaving the Grid
        //        Console.WriteLine("Mouse released after leaving the Grid.");
        //}

    }
}
