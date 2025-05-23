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

        public static List<HistoryItem> HistoryItems
        {
            get
            {
                string json = Interaction.GetSetting(AppName, Section, Key);
                if (!string.IsNullOrEmpty(json))
                {
                    var history = JsonSerializer.Deserialize<List<HistoryItem>>(json);
                    return history.OrderByDescending(h => h.Date).ToList();
                }
                   
                return new List<HistoryItem>();
            }
        }

        public static RelayCommand<HistoryItem> RemoveHistoryItemCommand =>
            new RelayCommand<HistoryItem>(value => RemoveHistoryItem(value.Path));

        public static void AddHistoryItem(string path)
        {           
            var twoWeeksAgo = DateTime.Now.AddDays(-14);
            var current = HistoryItems;
            current.RemoveAll(b => b.Path == path || b.Date < twoWeeksAgo);

            current.Add(new HistoryItem
            {
                Path = path,
                Title = System.IO.Path.GetFileName(path),
                HebrewDateTime = HebrewDateHelper.GetHebrewDateTime(DateTime.Now),
                Date = DateTime.Now
                
            });

            SaveHistoryItems(current);
        }


        public static void RemoveHistoryItem(string path)
        {
            var current = HistoryItems;
            var twoWeeksAgo = DateTime.Now.AddDays(-14);
            if (current.RemoveAll(b => b.Path == path || b.Date < twoWeeksAgo) > 0)
                SaveHistoryItems(current);
        }

        private static void SaveHistoryItems(List<HistoryItem> HistoryItems)
        {
            string json = JsonSerializer.Serialize(HistoryItems);
            Interaction.SaveSetting(AppName, Section, Key, json);
            OnStaticPropertyChanged(nameof(HistoryItems));
        }
    }
}
