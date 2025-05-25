using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace HelperConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var files = Directory.GetFiles("C:\\Users\\Admin\\Desktop\\תורת אמת", "*ללא ניקוד*", SearchOption.AllDirectories);
            //foreach (var file in files)
            //    File.Delete(file);


            //var files = Directory.GetFiles(@"C:\Users\Admin\Desktop\Otzarnik\01_תנך", "*", SearchOption.AllDirectories);
            //foreach (var file in files)
            //{
            //    string fileName = Path.GetFileNameWithoutExtension(file);
            //    string extension = Path.GetExtension(file);
            //    string folder = Path.GetDirectoryName(file);
            //    string foldername = Path.GetFileName(folder);

            //    string parentName = Regex.Replace(foldername, @"(\d+_)|(חומש)|(מסכת) *", "").Trim();
            //    if (!fileName.Contains(parentName))
            //    {
            //        string newFileName = $"{fileName} - {parentName}";
            //        string newPath = Path.Combine(folder, newFileName + extension);

            //        File.Move(file, newPath);
            //        Console.WriteLine($"Renamed:\n{file}\n→ {newPath}\n");
            //    }
            //}

            //List<string> snippets = new List<string>();
            //Regex regex = new Regex(@"כי ביצחק יקרא לך זרע", RegexOptions.Compiled);
            //var files = Directory.GetFiles("C:\\אוצריא\\אוצריא", "*.txt", SearchOption.AllDirectories).ToList();

            //const int snippetContextLength = 30; // number of characters before and after the match

            //for (int i = 0; i < files.Count; i++)
            //{
            //    string content = File.ReadAllText(files[i]);
            //    MatchCollection matchCollection = regex.Matches(content);

            //    Console.WriteLine($"{i + 1} / {files.Count} - {Path.GetFileName(files[i])}");

            //    foreach (Match match in matchCollection)
            //    {
            //        int start = Math.Max(0, match.Index - snippetContextLength);
            //        int length = Math.Min(content.Length - start, match.Length + snippetContextLength * 2);

            //        string snippet = content.Substring(start, length)
            //                                .Replace("\n", " ")
            //                                .Replace("\r", " ");

            //    }

            //    foreach (string snippet in snippets)
            //     Console.WriteLine($"  Snippet: ...{snippet}...");
            //}

            // Example user pattern with wildcards


            string userPattern = "כי * יקרא?לך|זרע"; // Example with wildcards

            string ConvertWildcardToRegex(string pattern)
            {
                // Escape all characters to make them literal
                string escaped = Regex.Escape(pattern);
                // Replace the escaped wildcards with actual regex equivalents
                escaped = escaped.Replace(@"\*", ".*").Replace(@"\?", ".").Replace(@"\|", "|");
                return $"({escaped})"; // Optional: group the pattern
            }

            var snippets = new ConcurrentBag<string>();
            Regex regex = new Regex(ConvertWildcardToRegex(userPattern), RegexOptions.Compiled);
            var files = Directory.GetFiles("C:\\אוצריא\\אוצריא", "*.txt", SearchOption.AllDirectories).ToList();
            const int snippetContextLength = 30;
            int progress = 0;

            Parallel.ForEach(files, (file) =>
            {
                try
                {
                    string content = RemoveDiacritics(File.ReadAllText(file));
                    MatchCollection matchCollection = regex.Matches(content);

                    int current = Interlocked.Increment(ref progress);
                    Console.WriteLine($"{current} / {files.Count} - {Path.GetFileName(file)}");

                    foreach (Match match in matchCollection)
                    {
                        int start = Math.Max(0, match.Index - snippetContextLength);
                        int length = Math.Min(content.Length - start, match.Length + snippetContextLength * 2);

                        string snippet = content.Substring(start, length)
                                                .Replace("\n", " ")
                                                .Replace("\r", " ");
                        snippets.Add(snippet);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error reading {file}: {ex.Message}");
                }
            });

            foreach (string snippet in snippets)
                Console.WriteLine($"  Snippet: ...{snippet}...");


            string RemoveDiacritics(string text)
            {
                var normalized = text.Normalize(NormalizationForm.FormD);
                var filtered = new string(normalized
                    .Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    .ToArray());
                return filtered.Normalize(NormalizationForm.FormC); // Optional re-normalization
            }
        }
    }
}
