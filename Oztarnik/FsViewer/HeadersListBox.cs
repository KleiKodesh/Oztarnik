using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfLib.ViewModels;

namespace Otzarnik.FsViewer
{
    internal class HeadersListView : FsListView
    {
        private CancellationTokenSource _cancellationTokenSource;
        public RelayCommand<TreeItem> NavigateCommand => new RelayCommand<TreeItem>(item => Navigate(item));

        public virtual void Navigate(TreeItem item)
        {
            if (item != null)
                OnNavigationRequested(item);
        }

        public async override void Search(string searchTerm) =>
            await SearchAsync(searchTerm);

        public async Task SearchAsync(string searchTerm)
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    Reset();
                    return;
                }

                var searchTerms = Regex.Split(searchTerm, @"[^\w""]+", RegexOptions.None)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();

                var items = Root.EnumerateItems().ToList();

                 var results = items
                        .Where(item => searchTerms.All(term => item.Name.Split(' ').Contains(term)));

                if (!results.Any())
                    results = items
                      .Where(item => searchTerms.All(term =>
                           item.Name.Split(' ').Concat(item.Tags .SelectMany(tag => tag.Split(' ')))
                              .Contains(term, StringComparer.OrdinalIgnoreCase)))
                                .OrderByDescending(x => GetScore(x, searchTerms));

                if (!results.Any())
                    results = items
                        .Select(item => new { Item = item, Score = GetScore(item, searchTerms) })
                        .Where(x => x.Score > 0)
                        .OrderByDescending(x => x.Score)
                        .Select(x => x.Item)
                        .Take(100)
                        .ToList();

                if (results.Any())
                {
                    CurrentItem = new TreeItem { Items = new ObservableCollection<TreeItem>() };
                    foreach (var item in results)
                    {
                        await Task.Delay(1, token);
                        token.ThrowIfCancellationRequested();
                        await Dispatcher.InvokeAsync(() =>
                            CurrentItem.AddChild(item));
                    }
                }
            }
            catch (OperationCanceledException) { }
        }

        private int GetScore(TreeItem item, string[] terms)
        {
            int nameScore = terms.Sum(term =>
            {
                if (item.Name == term) return 10;
                if (item.Name.StartsWith(term)) return 6;
                if (item.Name.Contains(term)) return 1;
                return 0;
            });

            string fullName = string.Join("", item.Tags) + item.Name;
            return nameScore + GetOrderedMatchScore(fullName, terms);
        }

        private int GetOrderedMatchScore(string text, string[] terms)
        {
            int index = 0;
            int score = 0;

            foreach (var term in terms)
            {
                int found = text.IndexOf(term, index);
                if (found >= 0)
                {
                    score += 4; 
                    index = found + term.Length;
                }
                else
                {
                    return 0;
                }
            }

            return score;
        }

    }
}
