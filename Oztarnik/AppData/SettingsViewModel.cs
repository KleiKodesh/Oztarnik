using Microsoft.VisualBasic;
using Ookii.Dialogs.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Forms;
using System.Windows.Media;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.AppData
{
    public static class Settings
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName)
        {
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));
        }

        const string AppName = "Otzarnik";
        const string Section = "Settings";
        static bool _doNotChangeDocumentColors = bool.TryParse(Interaction.GetSetting(AppName, Section, "DocumentColors"), out bool value) && value;
        static string _otzarnikFolder = Interaction.GetSetting(AppName, Section, "OtzarnikFolder", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "אוצרניק"));
        static string _otzariaFolder = Interaction.GetSetting(AppName, Section, "OtzariaFolder", "C:\\אוצריא\\אוצריא");
        static string _defaultFont = Interaction.GetSetting(AppName, Section, "DeafultFont", "Times New Roman");
        static int _defaultFontSize = int.Parse(Interaction.GetSetting(AppName, Section, "DefaultFontSize", "16"));

        public static string OtzarnikFolder
        {
            get => _otzarnikFolder;
            set
            {
                if (value == _otzarnikFolder) return;
                _otzarnikFolder = value;
                Interaction.SaveSetting(AppName, Section, "OtzarnikFolder", value);
                OnStaticPropertyChanged(nameof(OtzarnikFolder));
                OnStaticPropertyChanged(nameof(SourceFolders));
            }
        }

        public static string OtzariaFolder
        {
            get => _otzariaFolder;
            set
            {
                if (value == _otzariaFolder) return;
                _otzariaFolder = value;
                Interaction.SaveSetting(AppName, Section, "OtzariaFolder", value);
                OnStaticPropertyChanged(nameof(OtzariaFolder));
                OnStaticPropertyChanged(nameof(SourceFolders));
            }
        }

        public static ObservableCollection<string> SourceFolders => new ObservableCollection<string> { OtzarnikFolder, OtzariaFolder };

        public static List<FontFamily> Fonts => FontsHelper.FontsCollection;

        public static string DefaultFont
        {
            get => _defaultFont;
            set
            {
                if (value == _defaultFont) return;
                _defaultFont = value;
                Interaction.SaveSetting(AppName, Section, "DeafultFont", value ?? "Times New Roman");
                OnStaticPropertyChanged(nameof(DefaultFont));
            }
        }

        public static int DefaultFontSize
        {
            get => _defaultFontSize;
            set
            {
                if (value == _defaultFontSize) return;
                _defaultFontSize = value;
                Interaction.SaveSetting(AppName, Section, "DefaultFontSize", value.ToString());
                OnStaticPropertyChanged(nameof(DefaultFontSize));
            }
        }

        public static bool DoNotChangeDocumentColors
        {
            get => _doNotChangeDocumentColors;
            set
            {
                if (value == _doNotChangeDocumentColors) return;
                _doNotChangeDocumentColors = value;
                Interaction.SaveSetting(AppName, Section, "DocumentColors", value.ToString());
                OnStaticPropertyChanged(nameof(DoNotChangeDocumentColors));
            }
        }

        public static RelayCommand<string> SetFolderCommand => new RelayCommand<string>(targetFolder => SetFolder(targetFolder));
        public static RelayCommand SetFontCommand => new RelayCommand(SetFont);

        static void SetFolder(string targetFolder)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "בחר תיקייה";
            dialog.UseDescriptionForTitle = true;
            var result = dialog.ShowDialog();


            if (result == DialogResult.OK)
                Interaction.SaveSetting(AppName, Section, targetFolder, dialog.SelectedPath);

            if (targetFolder == "OtzarnikFolder")  OtzarnikFolder = dialog.SelectedPath;
            else if (targetFolder == "OtzariaFolder") OtzariaFolder = dialog.SelectedPath;
        }

        static void SetFont()
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
