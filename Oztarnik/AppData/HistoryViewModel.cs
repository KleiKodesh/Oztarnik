using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.AppData
{
    public class HistoryItem 
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public string HebrewDateTime { get; set; }
        public DateTime Date { get; set; }

        public override string ToString() => Title;
    }

    public static class HistoryViewModel
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName) =>
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));

        static string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppData");
        static string JsonPath = Path.Combine(DataPath, "History.json");

        static ObservableCollection<HistoryItem> _historyItems;

        public static ObservableCollection<HistoryItem> HistoryItems
        {
            get
            {
                if (_historyItems == null)
                    LoadItems();
                return _historyItems;
            }
            set
            {
                if (_historyItems == value) return;
                _historyItems = value;
                OnStaticPropertyChanged(nameof(HistoryItems));
            }
        }
        public static RelayCommand DeleteAllCommand =>
            new RelayCommand(DeleteAll);
        public static RelayCommand<HistoryItem> RemoveHistoryItemCommand =>
            new RelayCommand<HistoryItem>(value => RemoveHistoryItem(value.Path));


        static void LoadItems()
        {
            _historyItems = File.Exists(JsonPath) ?
            JsonSerializer.Deserialize<ObservableCollection<HistoryItem>>(File.ReadAllText(JsonPath)) :
                    new ObservableCollection<HistoryItem>();
        }

        public static void AddHistoryItem(string path)
        {           
            var twoWeeksAgo = DateTime.Now.AddDays(-14);
            HistoryItems.RemoveAll(b => b.Path == path || b.Date < twoWeeksAgo);

            string cleanedName = Regex.Replace(Path.GetFileName(path), @"^\d+_", "");

            var newItem = new HistoryItem
            {
                Path = path,
                Title = cleanedName,
                HebrewDateTime = HebrewDateHelper.GetHebrewDateTime(DateTime.Now),
                Date = DateTime.Now
            };

            HistoryItems.Add(newItem);

            Commit();
        }

        public static void RemoveHistoryItem(string path)
        {
            var twoWeeksAgo = DateTime.Now.AddDays(-14);
            HistoryItems.RemoveAll(b => b.Path == path || b.Date < twoWeeksAgo);
            Commit();
        }

        private static void DeleteAll()
        {
            HistoryItems = new ObservableCollection<HistoryItem>();
            Commit();
        }

        private static void Commit()
        {
            HistoryItems = new ObservableCollection<HistoryItem>(HistoryItems.OrderByDescending(item => item.Date));
            string json = JsonSerializer.Serialize(HistoryItems);
            if(!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
            File.WriteAllText(JsonPath, json);
            OnStaticPropertyChanged(nameof(HistoryItems));
        }
    }
}
