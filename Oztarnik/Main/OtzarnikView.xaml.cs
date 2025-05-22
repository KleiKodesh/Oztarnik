using Microsoft.VisualBasic;
using Ookii.Dialogs.WinForms;
using Otzarnik.FsViewer;
using System;
using System.Collections.Generic;
using System.Text.Json;
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
            fsViewer.SourceCollection = LoadPaths();
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
            {
                FileViewerTabControl.Items.Add(new TabItem 
                {
                    Header =  treeItem.Name,
                    Content = new FileViewer.FileView(treeItem),
                    IsSelected = true
                });
                MainTabControl.SelectedIndex = 1;
            }
        }

        private void FileViewerTabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is TabControl tabControl && tabControl.Items.Count == 0)
                MainTabControl.SelectedIndex = 0;
        }

        private void FolderPickerButton_Click(object sender, RoutedEventArgs e)
        {
            //var dialog = new VistaFolderBrowserDialog();
            //dialog.Description = "בחר תיקייה";
            //dialog.UseDescriptionForTitle = true; // Shows the description as the window title
            //var result = dialog.ShowDialog();

            //if (result == System.Windows.Forms.DialogResult.OK)
            //{
            //    var savedPaths = LoadPaths();
            //    savedPaths.Add(dialog.SelectedPath);
            //    string json = JsonSerializer.Serialize(savedPaths);
            //    Interaction.SaveSetting(AppDomain.CurrentDomain.BaseDirectory, "Tree", "SourceCollection", json);
            //}
        }

        private List<string> LoadPaths()
        {
            string json = Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Tree", "SourceCollection");

            if (!string.IsNullOrWhiteSpace(json))
            {
                var paths = JsonSerializer.Deserialize<List<string>>(json);
                if (paths != null)
                    return paths;
            }

            string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            return new List<string> { System.IO.Path.Combine(documentsPath, "אוצרניק"), @"C:\אוצריא\אוצריא" };
        }
    }
}
