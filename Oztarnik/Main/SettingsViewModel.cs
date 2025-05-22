using Microsoft.VisualBasic;
using Ookii.Dialogs.WinForms;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Forms;
using System.Windows.Media;
using WpfLib;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.Main
{
    public class SettingsViewModel : ViewModelBase
    {
        public string OtzarnikFolder => Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "OtzarnikFolder", System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "אוצרניק"));
        public string OtzariaFolder => Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "OtzariaFolder", "C:\\אוצריא\\אוצריא");
        public List<FontFamily> Fonts => FontsHelper.FontsCollection;
        
        public string DefaultFont
        {
            get => Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DeafultFont", "Times New Roman");
            set
            {
                OnPropertyChanged(nameof(DefaultFont));
                Interaction.SaveSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DeafultFont", value ?? "Times New Roman");
            }
        }

        public int DefaultFontSize
        {
            get => int.Parse(Interaction.GetSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DefaultFontSize", "16"));
            set => Interaction.SaveSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DefaultFontSize", value.ToString());
        }

        public ObservableCollection<string> SourceFolders  => 
            new ObservableCollection<string>  {  OtzarnikFolder, OtzariaFolder, };

        public RelayCommand<string> SetFolderCommand => new RelayCommand<string>(targetFolder => SetFolder(targetFolder));
        public RelayCommand SetFontCommand => new RelayCommand(SetFont);

        void SetFolder(string targetFolder)
        {
            var dialog = new VistaFolderBrowserDialog();
            dialog.Description = "בחר תיקייה";
            dialog.UseDescriptionForTitle = true; 
            var result = dialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
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
                Interaction.SaveSetting(AppDomain.CurrentDomain.BaseDirectory, "Settings", "DefaultFont", fontDialog.Font.Name);
            }
        }
    }
}
