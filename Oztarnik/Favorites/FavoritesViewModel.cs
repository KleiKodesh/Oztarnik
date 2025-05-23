using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using WpfLib;
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
    public static class FavoritesViewModel
    {
        public static event EventHandler<PropertyChangedEventArgs> StaticPropertyChanged;
        private static void OnStaticPropertyChanged(string propertyName) =>
            StaticPropertyChanged?.Invoke(null, new PropertyChangedEventArgs(propertyName));

        private static string AppName => AppDomain.CurrentDomain.BaseDirectory;
        private const string Section = "Favorites";
        private const string Key = "BookMarks";

        public static List<BookMarkModel> Bookmarks
        {
            get
            {
                string json = Interaction.GetSetting(AppName, Section, Key);
                if (!string.IsNullOrEmpty(json))
                    return JsonSerializer.Deserialize<List<BookMarkModel>>(json);
                return new List<BookMarkModel>();
            }
        }

        public static RelayCommand<BookMarkModel> RemoveBookMark =>
            new RelayCommand<BookMarkModel>(value => RemoveBookmark(value.Path));

        public static void AddBookmark(string path, string scrollIndex)
        {
            var inputBox = InputDialog(System.IO.Path.GetFileNameWithoutExtension(path));
            if (inputBox.DialogResult == true)
            {
                var current = Bookmarks;

                // Remove existing if the path already exists
                current.RemoveAll(b => b.Path == path);

                current.Add(new BookMarkModel
                {
                    Path = path,
                    ScrollIndex = scrollIndex,
                    Title = inputBox.Answer
                });

                SaveBookmarks(current);
            } 
        }

        public static void AddBookmark(BookMarkModel bookmark)
        {
            var current = Bookmarks;
            current.RemoveAll(b => b.Path == bookmark.Path);
            current.Add(bookmark);
            SaveBookmarks(current);
        }

        public static WpfLib.Controls.HebrewInputBox InputDialog(string defaultValue)
        {
            var inputBox = new WpfLib.Controls.HebrewInputBox("שמור סימניה", "כתוב כותרת:", defaultValue);
            inputBox.ShowDialog();
            return inputBox;
        }

        public static void RemoveBookmark(string path)
        {
            var current = Bookmarks;
            if (current.RemoveAll(b => b.Path == path) > 0)
                SaveBookmarks(current);
        }

        private static void SaveBookmarks(List<BookMarkModel> bookmarks)
        {
            string json = JsonSerializer.Serialize(bookmarks);
            Interaction.SaveSetting(AppName, Section, Key, json);
            OnStaticPropertyChanged(nameof(Bookmarks));
        }
    }
}
