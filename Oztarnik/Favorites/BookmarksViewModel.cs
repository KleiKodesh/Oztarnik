using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Text.RegularExpressions;
using WpfLib;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.Favorites
{
    public class BookMarkModel
    {
        public string Title { get; set; }
        public string Path { get; set; }
        public string ScrollIndex { get; set; }

        public override string ToString() => Title;
    }
    public static class BookmarksViewModel
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName) =>
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));

        private static string AppName => AppDomain.CurrentDomain.BaseDirectory;
        private const string Section = "Favorites";
        private const string Key = "BookMarks";

        static ObservableCollection<BookMarkModel> _bookmarks;
        
        public static ObservableCollection<BookMarkModel> Bookmarks
        {
            get
            {
                if (_bookmarks == null)
                {
                    string json = Interaction.GetSetting(AppName, Section, Key);
                    if (!string.IsNullOrEmpty(json))
                        _bookmarks = JsonSerializer.Deserialize<ObservableCollection<BookMarkModel>>(json);
                    else
                        _bookmarks = new ObservableCollection<BookMarkModel>();
                }

                return _bookmarks;
            }
            set
            {
                if (value != _bookmarks)
                {
                    _bookmarks = value;
                    OnStaticPropertyChanged(nameof(Bookmarks));
                }
            }
        }

        public static RelayCommand DeleteAllCommand =>
          new RelayCommand(DeleteAll);
        public static RelayCommand<BookMarkModel> RemoveBookMark =>
            new RelayCommand<BookMarkModel>(value => RemoveBookmark(value.Path));

        public static void AddBookmark(string path, string scrollIndex)
        {
            string cleanedName = Regex.Replace(System.IO.Path.GetFileName(path), @"^_\d+", "");

            var inputBox = InputDialog(cleanedName);
            if (inputBox.DialogResult == true)
            {
                Bookmarks.RemoveAll(b => b.Path == path);

                Bookmarks.Add(new BookMarkModel
                {
                    Path = path,
                    ScrollIndex = scrollIndex,
                    Title = inputBox.Answer
                });
            }

            SaveBookmarks();
        }

        public static void AddBookmark(BookMarkModel bookmark)
        {
            Bookmarks.RemoveAll(b => b.Path == bookmark.Path);
            Bookmarks.Add(bookmark);
            SaveBookmarks();
        }

        public static WpfLib.Controls.HebrewInputBox InputDialog(string defaultValue)
        {
            var inputBox = new WpfLib.Controls.HebrewInputBox("שמור סימניה", "כתוב כותרת:", defaultValue);
            inputBox.ShowDialog();
            return inputBox;
        }

        public static void RemoveBookmark(string path)
        {
            Bookmarks.RemoveAll(b => b.Path == path);
            SaveBookmarks();
        }

        private static void DeleteAll()
        {
            Bookmarks = new ObservableCollection<BookMarkModel>();
            SaveBookmarks();
        }

        private static void SaveBookmarks()
        {
            string json = JsonSerializer.Serialize(Bookmarks);
            Interaction.SaveSetting(AppName, Section, Key, json);
            OnStaticPropertyChanged(nameof(Bookmarks));
        }
    }
}
