using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using WpfLib.ViewModels;

namespace Otzarnik.FsViewer
{
    internal class HeadersListView : FsListView
    {
        public RelayCommand<TreeItem> NavigateCommand => new RelayCommand<TreeItem>(item => Navigate(item));

        public virtual void Navigate(TreeItem item)
        {
            if (item != null)
                OnNavigationRequested(item);
        }

        public override void Search(string searchTerm)
        {
          if (string.IsNullOrWhiteSpace(searchTerm)) { Reset(); return; }
            searchTerm = searchTerm.Replace("\"", "").Replace("\'", "").Replace("שולחן", "שלחן");
            var searchTerms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<TreeItem> results;

            var items = _root.EnumerateItems().ToList();

            results = items.Where(entry => searchTerms.All(
                term => entry.Name.Split(' ').Contains(term)))
                .OrderBy(t => t.Name.EndsWith(searchTerms.Last()));

            if (!results.Any())
                results = items.Where(entry => searchTerms.All(
                    term => entry.Name.Contains(term) ||
                    entry.Tags != null && entry.Tags.Any(t => t.Contains(term))))
                    .OrderBy(t => t.Name.EndsWith(searchTerms.Last()));

            if (results.Any())
                CurrentItem = new TreeItem { Items = new ObservableCollection<TreeItem>(results) };
        }
    }
}
