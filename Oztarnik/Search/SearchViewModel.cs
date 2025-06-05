using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using WpfLib.ViewModels;
using WpfLib;
using System.Windows;
using Otzarnik.FsViewer;
using Oztarnik.AppData;

namespace Otzarnik.Search
{
    public class SearchViewModel : ViewModelBase
    {
        private readonly string[] AllowedExtensions = { ".txt", ".html", ".htm" };

        private string _searchPattern;
        private double _progressValue;
        private int _totalFiles, _processedFiles;
        private DispatcherTimer _timer;
        private CancellationTokenSource _cts;
        private int _snippetLength = 140;
        const int ResultsCap = 500_000;
        static readonly Regex htmlCleanupRegex = new Regex(@"<[^>]*>|&[#a-zA-Z0-9]+;", RegexOptions.Compiled);

        public string SearchPattern  { get => _searchPattern; set => SetProperty(ref _searchPattern, value);}
        public double ProgressValue { get => _progressValue;   set => SetProperty(ref _progressValue, value);}
        public int SnippetLength  { get => _snippetLength;  set => SetProperty(ref _snippetLength, value);}

        public ObservableCollection<ResultModel> Results { get; } = new ObservableCollection<ResultModel>();

        public RelayCommand<TreeItem> SearchCommand => new RelayCommand<TreeItem>((item) => Search(item));
        public RelayCommand CancelSearchCommand => new RelayCommand(() => _cts?.Cancel());

        public async void Search(TreeItem treeItem)
        {
            if (treeItem == null) 
                return;

            try
            {
                _cts?.Cancel();
                while (_cts != null) await Task.Delay(1000);

                _cts = new CancellationTokenSource();
                Results.Clear();
                _processedFiles = 0;
                ProgressValue = 0;
                StartProgressTimer();

                int halfSnippet = SnippetLength / 2;
                var progress = new Progress<ResultCollection>(batch => ProcessResults(batch, halfSnippet));

                await Task.Run(() =>
                    SearchFiles(treeItem, _searchPattern, _cts.Token, progress));
            }
            catch (OperationCanceledException) { }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                StopProgressTimer();
                _cts = null;
                ProgressValue = 0;
            }
        }

        private void StartProgressTimer()
        {
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
            _timer.Tick += (s, e) =>
                ProgressValue = _totalFiles > 0 ? (_processedFiles * 100.0 / _totalFiles) : 0;
            _timer.Start();
        }

        private void StopProgressTimer()
        {
            _timer?.Stop();
            _timer = null;
        }

        private void SearchFiles(TreeItem treeItem, string pattern, CancellationToken token, IProgress<ResultCollection> progress)
        {
            var files = treeItem.EnumerateCheckedItems()
                .Where(f => f.IsFile == true && AllowedExtensions.Contains(f.Extension))
                .ToList();

            _totalFiles = files.Count;

            Regex regex = BuildFlexibleRegex(pattern);

            foreach (var file in files)
            {
                if (Results.Count > ResultsCap) return;
                token.ThrowIfCancellationRequested();

                if (!File.Exists(file.Path)) continue;
                string content = File.ReadAllText(file.Path);
                token.ThrowIfCancellationRequested();

                var results = regex.EnumerateMatches(content)
                                   .Select(m => new ResultModel { TreeItem = file, Match = m })
                                   .ToList();

                progress.Report(new ResultCollection { Content = content, Results = results });
                _processedFiles++;
            }
        }

        private Regex BuildFlexibleRegex(string pattern)
        {
            string escaped = Regex.Replace(pattern, @"[^|*?]", m => Regex.Escape(m.Value) + @"\p{Mn}*");
            escaped = Regex.Replace(escaped, @"(?<!\\p\{Mn\})\*", @"[\S\""]*?");
            return new Regex(escaped);
        }

        private void ProcessResults(ResultCollection batch, int snippetLength)
        {
            if (batch?.Results == null || batch.Content == null) return;

            foreach (var result in batch.Results)
            {
                if (result?.Match == null || result.Match.Value == null) continue;

                result.MatchValue = result.Match.Value.ReplaceShemHashem();
                result.MatchIndex = result.Match.Index;

                int preStart = Math.Max(0, result.MatchIndex - snippetLength);
                result.Pre = htmlCleanupRegex.Replace(batch.Content.Substring(preStart, result.MatchIndex - preStart), "")
                    .ReplaceShemHashem();

                int postStart = result.MatchIndex + result.MatchValue.Length;
                int postLength = Math.Min(snippetLength, batch.Content.Length - postStart);
                result.Post = postLength > 0 ? htmlCleanupRegex.Replace(batch.Content.Substring(postStart, postLength), "")
                    .ReplaceShemHashem() : string.Empty;

                result.Match = null;
                Results.Add(result);
            }
        }
    }
}
