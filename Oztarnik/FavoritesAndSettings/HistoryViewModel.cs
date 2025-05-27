using Microsoft.VisualBasic;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.FavoritesAndSettings
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

        private static string AppName => AppDomain.CurrentDomain.BaseDirectory;
        private const string Section = "Favorites";
        private const string Key = "History";

        static ObservableCollection<HistoryItem> _historyItems;

        public static ObservableCollection<HistoryItem> HistoryItems
        {
            get
            {
                if (_historyItems == null)
                {
                    string json = Interaction.GetSetting(AppName, Section, Key);
                    if (!string.IsNullOrEmpty(json))
                        _historyItems = JsonSerializer.Deserialize<ObservableCollection<HistoryItem>>(json);
                    else
                        _historyItems = new ObservableCollection<HistoryItem>();
                }

                return _historyItems;
            }
            set
            {
                if (value != _historyItems)
                {
                    _historyItems = value;
                    OnStaticPropertyChanged(nameof(HistoryItems));
                }
            }
        }

        public static RelayCommand DeleteAllCommand =>
            new RelayCommand(DeleteAll);
        public static RelayCommand<HistoryItem> RemoveHistoryItemCommand =>
            new RelayCommand<HistoryItem>(value => RemoveHistoryItem(value.Path));

        public static void AddHistoryItem(string path)
        {           
            var twoWeeksAgo = DateTime.Now.AddDays(-14);
            HistoryItems.RemoveAll(b => b.Path == path || b.Date < twoWeeksAgo);

            string cleanedName = Regex.Replace(System.IO.Path.GetFileName(path), @"^\d+_", "");

            var newItem = new HistoryItem
            {
                Path = path,
                Title = cleanedName,
                HebrewDateTime = HebrewDateHelper.GetHebrewDateTime(DateTime.Now),
                Date = DateTime.Now
            };

            HistoryItems.Add(newItem);

            SaveHistoryItems();
        }

        public static void RemoveHistoryItem(string path)
        {
            var twoWeeksAgo = DateTime.Now.AddDays(-14);
            HistoryItems.RemoveAll(b => b.Path == path || b.Date < twoWeeksAgo);
            SaveHistoryItems();
        }

        private static void DeleteAll()
        {
            HistoryItems = new ObservableCollection<HistoryItem>();
            SaveHistoryItems();
        }

        private static void SaveHistoryItems()
        {
            HistoryItems = new ObservableCollection<HistoryItem>(HistoryItems.OrderByDescending(item => item.Date));
            string json = JsonSerializer.Serialize(HistoryItems);
            Interaction.SaveSetting(AppName, Section, Key, json);
        }
    }
}
