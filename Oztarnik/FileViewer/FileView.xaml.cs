using System.Windows.Controls;
using System;
using Otzarnik.FsViewer;
using System.Windows.Input;
using System.Windows.Threading;
using Oztarnik.Main;
using Oztarnik.Favorites;
using System.Threading.Tasks;

namespace Oztarnik.FileViewer
{
    public partial class FileView : UserControl
    {
        public TreeItem TreeItem {get; set;}

        public FileView(TreeItem treeItem, string scrollIndex)
        {
            this.TreeItem = treeItem;
            InitializeComponent();
            LoadFile(treeItem, scrollIndex);
        }

        async void LoadFile(TreeItem treeItem, string scrollIndex)
        {
            var contentModel = await ContentParser.Parse(treeItem, true);
            headersListBox.Root = contentModel.RootHeader;           
            viewer.LoadDocument(contentModel.Content, scrollIndex);
            HistoryViewModel.AddHistoryItem(treeItem.Path);
            NavigationTextBox.Focus();  
        }

        private void headersListBox_NavigationRequested(object sender, System.Windows.RoutedEventArgs e)
        {
            if (e.OriginalSource is TreeItem item)
                viewer.NavigateToLine(item.LineIndex);
        }

        private void HeadersToggleButton_Checked(object sender, System.Windows.RoutedEventArgs e)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                NavigationTextBox.Focus();
                Keyboard.Focus(NavigationTextBox);
            }), DispatcherPriority.Input);
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
            FavoritesViewModel.AddBookmark(TreeItem.Path, scrollIndex);
        }

        public async Task<BookMarkModel> CreateBookMark()
        {
            return new BookMarkModel
            {
                Path = TreeItem.Path,
                ScrollIndex = await viewer.GetScrollIndex(),
            };
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
