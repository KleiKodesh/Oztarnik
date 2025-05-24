using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.Favorites
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

        static List<HistoryItem> _historyItems;

        public static List<HistoryItem> HistoryItems
        {
            get
            {
                if (_historyItems == null)
                {
                    string json = Interaction.GetSetting(AppName, Section, Key);
                    if (!string.IsNullOrEmpty(json))
                        _historyItems = JsonSerializer.Deserialize<List<HistoryItem>>(json);
                    else
                        _historyItems = new List<HistoryItem>();
                }

                return _historyItems;
            }
            set
            {
                if(value != _historyItems)
                {
                    _historyItems = value.OrderByDescending(h => h.Date).ToList();
                    SaveHistoryItems();
                }
            }
        }

        public static RelayCommand DeleteAllCommand =>
            new RelayCommand(DeleteAllHistoryItems);
        public static RelayCommand<HistoryItem> RemoveHistoryItemCommand =>
            new RelayCommand<HistoryItem>(value => RemoveHistoryItem(value.Path));

        public static void AddHistoryItem(string path)
        {           
            var twoWeeksAgo = DateTime.Now.AddDays(-14);
            HistoryItems.RemoveAll(b => b.Path == path || b.Date < twoWeeksAgo);

            HistoryItems.Add(new HistoryItem
            {
                Path = path,
                Title = System.IO.Path.GetFileName(path),
                HebrewDateTime = HebrewDateHelper.GetHebrewDateTime(DateTime.Now),
                Date = DateTime.Now
            });
        }

        public static void RemoveHistoryItem(string path)
        {
            var twoWeeksAgo = DateTime.Now.AddDays(-14);
            HistoryItems.RemoveAll(b => b.Path == path || b.Date < twoWeeksAgo);
        }

        private static void DeleteAllHistoryItems() =>
            HistoryItems = new List<HistoryItem>();

        private static void SaveHistoryItems()
        {
            string json = JsonSerializer.Serialize(HistoryItems);
            Interaction.SaveSetting(AppName, Section, Key, json);
            OnStaticPropertyChanged(nameof(HistoryItems));
        }
    }
}
