using System.Windows.Controls;
using System.IO;
using System;
using Otzarnik.FsViewer;
using System.Windows.Input;
using System.Windows.Threading;
using System.Windows;

namespace Oztarnik.FileViewer
{
    /// <summary>
    /// Interaction logic for FileView.xaml
    /// </summary>
    public partial class FileView : UserControl
    {
        public FileView(TreeItem treeItem)
        {
            InitializeComponent();
            LoadFile(treeItem);
        }

        async void LoadFile(TreeItem treeItem)
        {
            var contentModel = await ContentParser.Parse(treeItem, true);
            headersListBox.Root = contentModel.RootHeader;           
            viewer.LoadDocument(contentModel.Content);
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
    }
}
