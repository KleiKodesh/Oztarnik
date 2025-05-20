using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using WpfLib.Helpers;
using WpfLib.ViewModels;

namespace Otzarnik.FsViewer
{
    public class FsListBox : ListBox
    {
        private TreeItem _root;
        private TreeItem _previousItem;

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(
                nameof(Source),
                typeof(string),
                typeof(FsListBox),
                new PropertyMetadata(string.Empty, OnSourceChanged));

        public static readonly DependencyProperty SourceCollectionProperty =
            DependencyProperty.Register(
                nameof(SourceCollection),
                typeof(IEnumerable<string>),
                typeof(FsListBox),
                new PropertyMetadata(null, OnSourceCollectionChanged));

        public static readonly DependencyProperty CurrentItemProperty =
            DependencyProperty.Register(
                nameof(CurrentItem),
                typeof(TreeItem),
                typeof(FsListBox),
                new PropertyMetadata(null));

        public static readonly DependencyProperty SearchStringProperty =
          DependencyProperty.Register(
              nameof(SearchString),
              typeof(string),
              typeof(FsListBox),
              new PropertyMetadata(string.Empty, OnSearchStringChanged));

        public static readonly RoutedEvent NavigationRaisedEvent =
            EventManager.RegisterRoutedEvent(
                nameof(NavigationRaised),
                RoutingStrategy.Bubble,
                typeof(RoutedEventHandler),
                typeof(FsListBox));

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
            get => _root;
            set 
            {
                if (_root != value)
                {
                    _root = value;
                    CurrentItem = _root;
                }
            }
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


        public event RoutedEventHandler NavigationRaised
        {
            add => AddHandler(NavigationRaisedEvent, value);
            remove => RemoveHandler(NavigationRaisedEvent, value);
        }

        protected void OnNavigationRaised(TreeItem selectedItem) =>
            RaiseEvent(new RoutedEventArgs(NavigationRaisedEvent, selectedItem));

        private static void OnSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FsListBox viewer && e.NewValue is string newValue)
                viewer.Root = TreeHelper.BuildTree(null, newValue, newValue);
        }

        private static void OnSourceCollectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FsListBox viewer && e.NewValue is IEnumerable<string> sources)
            {
                var root = new TreeItem { Name = "Root" };

                foreach (var source in sources)
                {
                    var tree = TreeHelper.BuildTree(null, source, source);
                    if (tree != null)
                        root.AddChild(tree);
                }

                viewer.Root = root;
            }
        }


        private static void OnSearchStringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FsListBox viewer && e.NewValue is string newValue)
                viewer.Search(newValue);
        }

        // Commands
        public RelayCommand<TreeItem> GoToCommand => new RelayCommand<TreeItem>(item => Goto(item));
        public RelayCommand GoBackCommand => new RelayCommand(() => GoBack());
        public RelayCommand ResetCommand => new RelayCommand(() => Reset());
        public RelayCommand SearchCommand => new RelayCommand(() => Search(SearchString));
        public RelayCommand FocusCommand => new RelayCommand(() => SearchString = string.Empty);

        public FsListBox()
        {
            this.SelectionChanged += FsListBox_SelectionChanged;
            PreviewKeyDown += FsListBox_PreviewKeyDown;
        }

        private void FsListBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
                GoBack();
            else if (e.Key == Key.Home)
                Reset();
        }

        private void FsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedIndex < 0)
            {
                SelectedIndex = 0;
                return;
            }

            var item = SelectedItem;
            if (item != null)
            {
                var listBoxItem = ItemContainerGenerator.ContainerFromItem(item) as ListBoxItem;
                if (listBoxItem != null &&
                    DependencyHelper.FindChild<Button>(listBoxItem) is Button button)
                {
                    listBoxItem.Dispatcher.BeginInvoke(new Action(() =>
                    {
                        button.Focus();
                        Keyboard.Focus(button);
                    }), DispatcherPriority.Input);
                }
            }

        }

        public void GoBack() => CurrentItem = CurrentItem?.Parent ?? _previousItem ?? _root;
        public void Reset() => CurrentItem = _root;

        public virtual void Goto(TreeItem item)
        {
            if (item == null) 
                return;

            if (item.IsFile)
                OnNavigationRaised(item);         
            else
                CurrentItem = item;
        }

        public virtual void Search(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                Reset();
                return;
            }

            searchTerm = searchTerm.Replace("\"", "").Replace("\'", "").Replace("שולחן", "שלחן");
            var searchTerms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<TreeItem> results;

            if (searchTerms.Length == 1 && searchTerms[0].Length < 5)
            {
                results = _root.EnumerateItems().Where(c =>
                    c.Name.StartsWith(searchTerms[0]) ||
                    (c.Tags != null && c.Tags.Any(t => t.StartsWith(searchTerms[0]))));
            }
            else
            {
                results = _root.EnumerateItems().Where(entry => searchTerms.All(
                    term => entry.Name.Contains(term) ||
                    (entry.Tags != null && entry.Tags.Any(t => t.Contains(term)))));
            }

            if (results.Any())
                CurrentItem = new TreeItem { Items = new ObservableCollection<TreeItem>(results) };
        }
    }
}
