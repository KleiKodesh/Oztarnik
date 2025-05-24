using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text.Json;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.Favorites
{
    public class EnvironmentModel
    {
        public string Title { get; set; }
        public List<BookMarkModel> Bookmarks { get; set; } = new List<BookMarkModel>();
        public override string ToString() => Title;
    }

    public static class EnvironmentsViewModel
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName) =>
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));

        private static string AppName => AppDomain.CurrentDomain.BaseDirectory;
        private const string Section = "Favorites";
        private const string Key = "Environments";
        static ObservableCollection<EnvironmentModel> _environment;
       
        public static ObservableCollection<EnvironmentModel> Environments
        {
            get
            {
                if (_environment == null)
                {
                    string json = Interaction.GetSetting(AppName, Section, Key);
                    if (!string.IsNullOrEmpty(json))
                        _environment = JsonSerializer.Deserialize<ObservableCollection<EnvironmentModel>>(json);
                    else
                        _environment = new ObservableCollection<EnvironmentModel>();
                }

                return _environment;
            }
            set
            {
                if (value != _environment)
                {
                    _environment = value;
                    OnStaticPropertyChanged(nameof(Environments));
                }
            }
        }

        public static RelayCommand DeleteAllCommand =>
          new RelayCommand(DeleteAll);
        public static RelayCommand<EnvironmentModel> RemoveEnvironmentCommand =>
            new RelayCommand<EnvironmentModel>(value => RemoveEnvironment(value));

        public static void AddEnvironment(List<BookMarkModel> bookMarks)
        {
            var inputBox = InputDialog(HebrewDateHelper.GetHebrewDateTime(DateTime.Now));
            if (inputBox.DialogResult == true)
                Environments.Add(new EnvironmentModel { Title = inputBox.Answer, Bookmarks = bookMarks});

            SaveEnvironments();
        }

        public static WpfLib.Controls.HebrewInputBox InputDialog(string defaultValue)
        {
            var inputBox = new WpfLib.Controls.HebrewInputBox("שמור סביבת עבודה", "כתוב כותרת:", defaultValue);
            inputBox.ShowDialog();
            return inputBox;
        }

        public static void RemoveEnvironment(EnvironmentModel Environment)
        {
            Environments.RemoveAll(e => e.Title == Environment.Title);
            SaveEnvironments();
        }

        private static void DeleteAll()
        {
            Environments = new ObservableCollection<EnvironmentModel>();
            SaveEnvironments();
        }

        private static void SaveEnvironments()
        {
            string json = JsonSerializer.Serialize(Environments);
            Interaction.SaveSetting(AppName, Section, Key, json);
            OnStaticPropertyChanged(nameof(Environments));
        }
    }
}
