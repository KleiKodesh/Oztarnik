using Otzarnik.FsViewer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Oztarnik.FsViewer
{
    public static class FsSearch
    {
        public static IEnumerable<string> ExtractSearchTerms(string searchTerm)
        {
            return Regex.Split(searchTerm, @"[^\w""]+")
                        .Where(s => !string.IsNullOrWhiteSpace(s));
        }

        public static IEnumerable<TreeItem> Search(TreeItem root, string searchTerm)
        {
            try
            {
                var searchTerms = Regex.Split(searchTerm, @"[^\w""]+", RegexOptions.None)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .ToArray();

                var items = root.EnumerateItems().ToList();

                //var results = items
                //       .Where(item => searchTerms.All(term => item.Name.Split(' ').Contains(term)));

                //if (!results.Any())
                //{
                //    results = items
                //         .Where(item => searchTerms.All(term =>
                //             item.Name.Split(' ')
                //                 .Concat((item.Tags ?? new List<string>())
                //                     .Where(tag => tag != null) // <-- filter out null tags
                //                     .SelectMany(tag => tag.Split(' ')))
                //                 .Contains(term, StringComparer.OrdinalIgnoreCase)))
                //         .OrderByDescending(x => GetScore(x, searchTerms));
                //}


                //if (!results.Any())
                var results = items
                        .Select(item => new { Item = item, Score = GetScore(item, searchTerms) })
                        .Where(x => x.Score > 0)
                        .OrderByDescending(x => x.Score)
                        .Select(x => x.Item)
                        .Take(100)
                        .ToList();

                return results;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return new List<TreeItem>();
            }
        }

        private static int GetScore(TreeItem item, string[] terms)
        {
            //int nameScore = terms.Sum(term =>
            //{
            //    if (item.Name == term) 
            //        return 10;
            //    if (item.Name.StartsWith(term)) return 6;
            //    if (item.Name.Contains(term)) return 1;
            //    return 0;
            //});

            string fullName = string.Join("", item.Tags ?? Enumerable.Empty<string>()) + " " + item.Name;
            return /*nameScore + */GetOrderedMatchScore(fullName, terms);
        }

        private static int GetOrderedMatchScore(string text, string[] terms)
        {
            int index = 0;
            int score = 0;

            foreach (var term in terms)
            {
                int found = text.IndexOf(term, index);
                if (found >= 0)
                {
                    //score += 4;
                    var realpos = (index + found);
                    var earlyModifier = 1000 - realpos + term.Length;
                    score += earlyModifier;
                    index = found + term.Length;
                }
            }

            return score;
        }
    }
}
