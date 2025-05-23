using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.Favorites
{
    public class EnviromentModel
    {
        public string Title { get; set; }
        public List<BookMarkModel> Bookmarks { get; set; } = new List<BookMarkModel>();
        public override string ToString() => Title;
    }

    public static class EnviromentsViewModel
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName) =>
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));

        private static string AppName => AppDomain.CurrentDomain.BaseDirectory;
        private const string Section = "Favorites";
        private const string Key = "Enviroments";

        public static List<EnviromentModel> Enviroments
        {
            get
            {
                string json = Interaction.GetSetting(AppName, Section, Key);
                if (!string.IsNullOrEmpty(json))
                    return JsonSerializer.Deserialize<List<EnviromentModel>>(json);
                return new List<EnviromentModel>();
            }
        }

        public static RelayCommand<EnviromentModel> RemoveEnviromentCommand =>
            new RelayCommand<EnviromentModel>(value => RemoveEnviroment(value));

        public static void AddEnviroment(List<BookMarkModel> bookMarks)
        {
            var inputBox = InputDialog(HebrewDateHelper.GetHebrewDateTime(DateTime.Now));
            if (inputBox.DialogResult == true)
            {
                var current = Enviroments;
                current.Add(new EnviromentModel { Title = inputBox.Answer, Bookmarks = bookMarks});
                SaveEnviroments(current);
            }
        }

        public static WpfLib.Controls.HebrewInputBox InputDialog(string defaultValue)
        {
            var inputBox = new WpfLib.Controls.HebrewInputBox("שמור סביבת עבודה", "כתוב כותרת:", defaultValue);
            inputBox.ShowDialog();
            return inputBox;
        }

        public static void RemoveEnviroment(EnviromentModel enviroment)
        {
            var current = Enviroments;
            if (current.RemoveAll(e => e.Title == enviroment.Title) > 0)
                SaveEnviroments(current);
        }

        private static void SaveEnviroments(List<EnviromentModel> enviroments)
        {
            string json = JsonSerializer.Serialize(enviroments);
            Interaction.SaveSetting(AppName, Section, Key, json);
            OnStaticPropertyChanged(nameof(Enviroments));
        }
    }
}
