using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Oztarnik.AppData
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

        static string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AppData");
        static string JsonPath = Path.Combine(DataPath, "Bookmarks.json");

        public static ObservableCollection<BookMarkModel> _bookmarks;
        public static ObservableCollection<BookMarkModel> Bookmarks
        {
            get
            {
                if (_bookmarks == null)
                    LoadBookmarks();
                return _bookmarks;
            }
            set
            {
                if (_bookmarks == value) return;
                _bookmarks = value;
                OnStaticPropertyChanged(nameof(Bookmarks));
            }
        }

        public static RelayCommand DeleteAllCommand =>
          new RelayCommand(DeleteAll);
        public static RelayCommand<BookMarkModel> RemoveBookMark =>
            new RelayCommand<BookMarkModel>(value => RemoveBookmark(value.Path));

        static void LoadBookmarks()
        {
            _bookmarks = File.Exists(JsonPath) ?
            JsonSerializer.Deserialize<ObservableCollection<BookMarkModel>>(File.ReadAllText(JsonPath)) :
                    new ObservableCollection<BookMarkModel>();
        }

        public static void AddBookmark(string path, string scrollIndex)
        {
            string cleanedName = Regex.Replace(System.IO.Path.GetFileName(path), @"^_\d+", "");

            var inputBox = InputDialog(cleanedName);
            if (inputBox.DialogResult == true)
            {
                Bookmarks.RemoveAll(b => b.Path == path && b.Title == inputBox.Answer);

                Bookmarks.Add(new BookMarkModel
                {
                    Path = path,
                    ScrollIndex = scrollIndex,
                    Title = inputBox.Answer
                });
            }

            Commit();
        }

        public static void AddBookmark(BookMarkModel bookmark)
        {
            Bookmarks.RemoveAll(b => b.Path == bookmark.Path);
            Bookmarks.Add(bookmark);
            Commit();
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
            Commit();
        }

        private static void DeleteAll()
        {
            Bookmarks = new ObservableCollection<BookMarkModel>();
            Commit();
        }

        private static void Commit()
        {
            string json = JsonSerializer.Serialize(Bookmarks);
            if(!Directory.Exists(DataPath)) Directory.CreateDirectory(DataPath);
            File.WriteAllText(JsonPath, json);
            OnStaticPropertyChanged(nameof(Bookmarks));
        }
    }
}
