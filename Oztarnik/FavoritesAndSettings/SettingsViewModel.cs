using Microsoft.VisualBasic;
using Ookii.Dialogs.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Windows.Forms;
using System.Windows.Media;
using WpfLib;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.FavoritesAndSettings
{
    public static class Settings
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        private static bool _doNotChangeDocumentColors = bool.Parse(Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DocumentColors", "true"));

        public static bool DoNotChangeDocumentColors
        {
            get => _doNotChangeDocumentColors;
            set
            {
                _doNotChangeDocumentColors = value;
                Interaction.SaveSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DocumentColors", value.ToString());
                OnStaticPropertyChanged(nameof(DoNotChangeDocumentColors));
            }
        }
    }

    public class SettingsViewModel : ViewModelBase
    {
        public string OtzarnikFolder => Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "OtzarnikFolder", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Otzarnik"));
        public string OtzariaFolder => Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "OtzariaFolder", "C:\\אוצריא\\אוצריא");
        public List<FontFamily> Fonts => FontsHelper.FontsCollection;

        string _defaultFont = Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DeafultFont", "Times New Roman");
        int _defaultFontSize = int.Parse(Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DefaultFontSize", "16"));
        
        public string DefaultFont
        {
            get => _defaultFont;
            set
            {
                if (value == _defaultFont) return;
                _defaultFont = value;
                Interaction.SaveSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DeafultFont", value ?? "Times New Roman");
                OnPropertyChanged(nameof(DefaultFont));
            }
        }

        public int DefaultFontSize
        {
            get => _defaultFontSize;
            set
            {
                if (value == _defaultFontSize) return;
                _defaultFontSize = value;
                Interaction.SaveSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DefaultFontSize", value.ToString());
                OnPropertyChanged(nameof(DefaultFontSize));
            }
        }

        public ObservableCollection<string> SourceFolders =>
            new ObservableCollection<string> { OtzarnikFolder, OtzariaFolder, };

        public RelayCommand<string> SetFolderCommand => new RelayCommand<string>(targetFolder => SetFolder(targetFolder));
        public RelayCommand SetFontCommand => new RelayCommand(SetFont);

        void SetFolder(string targetFolder)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "בחר תיקייה";
            dialog.UseDescriptionForTitle = true;
            var result = dialog.ShowDialog();

            if (result == DialogResult.OK)
                Interaction.SaveSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", targetFolder, dialog.SelectedPath);

            OnPropertyChanged(nameof(SourceFolders));
            OnPropertyChanged(nameof(OtzariaFolder));
            OnPropertyChanged(nameof(OtzarnikFolder));
        }

        void SetFont()
        {
            FontDialog fontDialog = new FontDialog();
            fontDialog.ShowColor = false;
            fontDialog.ShowEffects = false;
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                DefaultFont = fontDialog.Font.Name;
            }
        }
    }
}
