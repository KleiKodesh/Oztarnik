using Oztarnik.FsViewer;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Otzarnik.FsViewer
{
    public class FsListView : ListView
    {
        private TreeItem _previousItem;
        private CancellationTokenSource _cancellationTokenSource;

        public static readonly DependencyProperty RootProperty =
            DependencyProperty.Register(
                nameof(Root),
                typeof(TreeItem),
                typeof(FsListView),
                new PropertyMetadata(null));


        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(string),
                typeof(FsListView),
                new PropertyMetadata(string.Empty, OnSourceChanged));

        public static readonly DependencyProperty SourceCollectionProperty =
            DependencyProperty.Register(
                nameof(SourceCollection),
                typeof(IEnumerable<string>),
                typeof(FsListView),
                new PropertyMetadata(null, OnSourceCollectionChanged));

        public static readonly DependencyProperty CurrentItemProperty =
            DependencyProperty.Register(
                nameof(CurrentItem),
                typeof(TreeItem),
                typeof(FsListView),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SearchStringProperty =
          DependencyProperty.Register(
              nameof(SearchString),
              typeof(string),
              typeof(FsListView),
              new PropertyMetadata(string.Empty, OnSearchStringChanged));

        public static readonly RoutedEvent NavigationRequestedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(NavigationRequested),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(FsListView));

        public string Source
        {
            get => (string)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public IEnumerable<string> SourceCollection
        {
            get => (IEnumerable<string>)GetValue(SourceCollectionProperty);
            set => SetValue(SourceCollectionProperty, value);
        }

        public TreeItem Root
        {
            get => (TreeItem)GetValue(RootProperty);
            set { SetValue(RootProperty, value); CurrentItem = value; }
        }


        public TreeItem CurrentItem
        {
            get => (TreeItem)GetValue(CurrentItemProperty);
            set
            {
                _previousItem = CurrentItem;
                SetValue(CurrentItemProperty, value);
                ItemsSource = value.Items;
            }
        }

        public string SearchString
        {
            get => (string)GetValue(SearchStringProperty);
            set => SetValue(SearchStringProperty, value);
        }


        public event RoutedEventHandler NavigationRequested
        {
            add => AddHandler(NavigationRequestedEvent, value);
            remove => RemoveHandler(NavigationRequestedEvent, value);
        }

        protected void OnNavigationRequested(TreeItem selectedItem) =>
            RaiseEvent(new RoutedEventArgs(NavigationRequestedEvent, selectedItem));

        private async static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FsListView viewer && e.NewValue is string newValue &&
                Directory.Exists(newValue))
                    await viewer.Dispatcher.InvokeAsync(() =>
                        viewer.Root = TreeHelper.BuildTree(null, newValue, newValue), 
                            DispatcherPriority.ApplicationIdle);
        }

        private async static void OnSourceCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FsListView viewer && e.NewValue is IEnumerable<string> sources)
            {
                await viewer.Dispatcher.InvokeAsync(() =>
                {
                    viewer.Root = new TreeItem { Name = "Root" };

                    foreach (var source in sources)
                    {
                        if (Directory.Exists(source))
                        {
                            var tree = TreeHelper.BuildTree(null, source, source);
                            if (tree != null)
                                viewer.Root.AddChild(tree);
                        }
                    }
                }, DispatcherPriority.ApplicationIdle);
            }
        }


        private static void OnSearchStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FsListView viewer && e.NewValue is string newValue)
                viewer.Search(newValue);
        }

        // Commands
        public RelayCommand<TreeItem> GoToCommand => new RelayCommand<TreeItem>(item => Goto(item));
        public RelayCommand GoBackCommand => new RelayCommand(() => GoBack());
        public RelayCommand ResetCommand => new RelayCommand(() => Reset());
        public RelayCommand SearchCommand => new RelayCommand(() => Search(SearchString));
        public RelayCommand FocusCommand => new RelayCommand(() => SearchString = string.Empty);

        public FsListView()
        {
            this.SelectionChanged += FsListView_SelectionChanged;
            PreviewKeyDown += FsListView_PreviewKeyDown;
        }

        private void FsListView_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                GoBack();
            else if (e.Key == Key.Home)
                Reset();
        }

        private void FsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedIndex < 0)
            {
                SelectedIndex = 0;
                return;
            }

            var item = SelectedItem;
            if (item != null)
            {
                var ListViewItem = ItemContainerGenerator.ContainerFromItem(item) as ListViewItem;
                if (ListViewItem != null)
                {
                    if (DependencyHelper.FindChild<Button>(ListViewItem) is Button button)
                    {
                        ListViewItem.Dispatcher.BeginInvoke(new Action(() =>
                        {
                            button.Focus();
                            Keyboard.Focus(button);
                        }), DispatcherPriority.Input);
                    }
                    ListViewItem.BringIntoView();
                }
            }
        }

        public void GoBack() => CurrentItem = CurrentItem?.Parent ?? _previousItem ?? Root;
        public void Reset() => CurrentItem = Root;

        public virtual void Goto(TreeItem item)
        {
            if (item == null) 
                return;

            if (item.IsFile)
                OnNavigationRequested(item);         
            else
                CurrentItem = item;
        }

        public async void Search(string searchTerm)
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

                var results = FsSearch.Search(Root, searchTerm);

                if (results.Any())
                {
                    CurrentItem = new TreeItem { Items = new ObservableCollection<TreeItem>() };

                    foreach (var item in results)
                    {
                        await Task.Delay(1, token); // keeps UI responsive
                        token.ThrowIfCancellationRequested();
                        await Dispatcher.InvokeAsync(() => CurrentItem.AddChild(item));
                    }
                }
            }
            catch (OperationCanceledException) { }
        }


        //public async Task SearchAsync(string searchTerm)
        //{
        //    _cancellationTokenSource?.Cancel();
        //    _cancellationTokenSource = new CancellationTokenSource();
        //    var token = _cancellationTokenSource.Token;

        //    try
        //    {
        //        if (string.IsNullOrWhiteSpace(searchTerm))
        //        {
        //            Reset();
        //            return;
        //        }

        //        var searchTerms = Regex.Split(searchTerm, @"[^\w""]+", RegexOptions.None)
        //                .Where(s => !string.IsNullOrWhiteSpace(s))
        //                .ToArray();

        //        var items = Root.EnumerateItems().ToList();

        //        var results = items
        //               .Where(item => searchTerms.All(term => item.Name.Split(' ').Contains(term)));

        //        if (!results.Any())
        //        {
        //            results = items
        //                .Where(item => searchTerms.All(term =>
        //                    item.Name.Split(' ')
        //                        .Concat((item.Tags ?? Enumerable.Empty<string>()).SelectMany(tag => tag.Split(' ')))
        //                        .Contains(term, StringComparer.OrdinalIgnoreCase)))
        //                .OrderByDescending(x => GetScore(x, searchTerms));
        //        }


        //        if (!results.Any())
        //            results = items
        //                .Select(item => new { Item = item, Score = GetScore(item, searchTerms) })
        //                .Where(x => x.Score > 0)
        //                .OrderByDescending(x => x.Score)
        //                .Select(x => x.Item)
        //                .Take(100)
        //                .ToList();

        //        if (results.Any())
        //        {
        //            CurrentItem = new TreeItem { Items = new ObservableCollection<TreeItem>() };
        //            foreach (var item in results)
        //            {
        //                await Task.Delay(1, token);
        //                token.ThrowIfCancellationRequested();
        //                await Dispatcher.InvokeAsync(() =>
        //                    CurrentItem.AddChild(item));
        //            }
        //        }
        //    }
        //    catch (OperationCanceledException) { }
        //}

        //private int GetScore(TreeItem item, string[] terms)
        //{
        //    int nameScore = terms.Sum(term =>
        //    {
        //        if (item.Name == term) return 10;
        //        if (item.Name.StartsWith(term)) return 6;
        //        if (item.Name.Contains(term)) return 1;
        //        return 0;
        //    });

        //    string fullName = string.Join("", item.Tags ?? Enumerable.Empty<string>()) + item.Name;
        //    return nameScore + GetOrderedMatchScore(fullName, terms);
        //}

        //private int GetOrderedMatchScore(string text, string[] terms)
        //{
        //    int index = 0;
        //    int score = 0;

        //    foreach (var term in terms)
        //    {
        //        int found = text.IndexOf(term, index);
        //        if (found >= 0)
        //        {
        //            score += 4;
        //            index = found + term.Length;
        //        }
        //        else
        //        {
        //            return 0;
        //        }
        //    }

        //    return score;
        //}
    }
}
