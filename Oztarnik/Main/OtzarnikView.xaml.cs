using Otzarnik.FsViewer;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Oztarnik.Main
{
    /// <summary>
    /// Interaction logic for OtzarnikView.xaml
    /// </summary>
    public partial class OtzarnikView : UserControl
    {
        public OtzarnikView()
        {
            InitializeComponent();
            var list = new List<string>
            {
                "C:\\Users\\Admin\\Desktop\\תורת אמת",
                "C:\\אוצריא\\אוצריא"
            };

            fsViewer.SourceCollection = list;
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
            {
                FileViewerTabControl.Items.Add(new TabItem 
                {
                    Header =  treeItem.Name,
                    Content = new FileViewer.FileView(treeItem)
                });
                MainTabControl.SelectedIndex = 1;
            }
        }

        private void FileViewerTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl && tabControl.Items.Count == 0)
                MainTabControl.SelectedIndex = 0;
        }
    }
}
