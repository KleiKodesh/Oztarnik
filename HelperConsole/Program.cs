using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace HelperConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            string pattern = "כי ביצחק";
            pattern = Regex.Replace(pattern, @"[^|*?]", m => Regex.Escape(m.Value) + @"\p{Mn}*");
            pattern = Regex.Replace(pattern, @"(?<!\\p\{Mn\})\*", @"[\S\""]*?");

            var files = Directory.GetFiles(@"C:\אוצריא\אוצריא", "*.txt", SearchOption.AllDirectories);
            Console.WriteLine($"Total files: {files.Length}");

            var sw = new Stopwatch();

            // Benchmark ReadAllText
            sw.Start();
            foreach (var file in files)
            {
                var content = File.ReadAllText(file);
                var matches = Regex.Matches(content, pattern);
            }
            sw.Stop();
            Console.WriteLine("ReadAllText: " + sw.Elapsed);

            // Benchmark ReadLines
            sw.Reset();
            sw.Start();
            foreach (var file in files)
            {
                foreach (var line in File.ReadLines(file))
                {
                    var matches = Regex.Matches(line, pattern);
                }
            }
            sw.Stop();
            Console.WriteLine("ReadLines: " + sw.Elapsed);
        }
    }
}
