using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.AppData
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

        static string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppData");
        static string JsonPath = Path.Combine(DataPath, "Environments.json");

        public static ObservableCollection<EnvironmentModel> _environments;
        public static ObservableCollection<EnvironmentModel> Environments
        {
            get
            {
                if (_environments == null)
                    LoadEnvironments();
                return   _environments;
            }
            set
            {
                if (value == _environments) return;
                _environments = value;
                OnStaticPropertyChanged(nameof(Environments));
            }
        }

        public static RelayCommand DeleteAllCommand => new RelayCommand(DeleteAll);
        public static RelayCommand<EnvironmentModel> RemoveEnvironmentCommand =>
            new RelayCommand<EnvironmentModel>(value => RemoveEnvironment(value));

        static void LoadEnvironments()
        {
            _environments = File.Exists(JsonPath) ?
            JsonSerializer.Deserialize<ObservableCollection<EnvironmentModel>>(File.ReadAllText(JsonPath)) :
                    new ObservableCollection<EnvironmentModel>();
        }

        public static void AddEnvironment(List<BookMarkModel> bookMarks)
        {
            var inputBox = InputDialog(HebrewDateHelper.GetHebrewDateTime(DateTime.Now));
            if (inputBox.DialogResult == true)
                Environments.Add(new EnvironmentModel { Title = inputBox.Answer, Bookmarks = bookMarks});

            Commit();
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
            Commit();
        }

        private static void DeleteAll()
        {
            Environments = new ObservableCollection<EnvironmentModel>();
            Commit();
        }

        private static void Commit()
        {
            string json = JsonSerializer.Serialize(Environments);
            if (!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
            File.WriteAllText(JsonPath, json);
            OnStaticPropertyChanged(nameof(Environments));
        }
    }
}
