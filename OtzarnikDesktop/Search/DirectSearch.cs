//using System;
//using System.IO;
//using System.Linq;
//using System.Text.RegularExpressions;
//using System.Threading;

//namespace Otzarnik.Search
//{
//    public static class DirectSearch
//    {
//        public static void SearchFiles(string[] files, string pattern, CancellationToken token, IProgress<ResultCollection> progress)
//        {
//            Regex regex = BuildRegex(pattern);

//            foreach (var file in files)
//            {
//                token.ThrowIfCancellationRequested();

//                string content = File.ReadAllText(file);
//                token.ThrowIfCancellationRequested();

//                var results = regex.EnumerateMatches(content)
//                                   .Select(m => new ResultModel { FilePath = file, Match = m });

//                progress.Report(new ResultCollection { Content = content, Results = results });
//            }
//        }

//        private static Regex BuildRegex(string pattern)
//        {
//            string escaped = Regex.Replace(pattern, @"[^|*?]", m => Regex.Escape(m.Value) + @"\p{Mn}*");
//            escaped = Regex.Replace(escaped, @"(?<!\\p\{Mn\})\*", @"[\S\""]*?");
//            return new Regex(escaped);
//        }
//    }
//}
