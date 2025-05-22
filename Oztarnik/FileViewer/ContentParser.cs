using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Otzarnik.FsViewer
{
    public class FileContentModel
    {
        public TreeItem TreeItem { get; set; }
        public string Content { get; set; }
        public TreeItem RootHeader {get; set;} = new TreeItem();
    }
    
    public static class ContentParser
    {
        public static Task <FileContentModel> Parse (TreeItem treeItem, bool getContent)
        {
            FileContentModel result = new FileContentModel { TreeItem = treeItem };

            var stb = new StringBuilder();
            Stack<TreeItem> headerStack = new Stack<TreeItem>();            
            int lineIndex = -1;

            if (!File.Exists(treeItem.Path))
                return Task.FromResult(result);

            foreach (var line in File.ReadLines(treeItem.Path))
            {
                lineIndex++;

                if (getContent && !string.IsNullOrWhiteSpace(line.Trim())) 
                    stb.AppendLine($"<line>{line}</line>");

                Match match = Regex.Match(line.TrimStart(), @"^<(h\d+|ot|header).*?>([^\n\r<]+?)</", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string header = match.Groups[1].Value;

                    if (header == "header")
                    {
                        TreeItem node = new TreeItem
                        {
                            Name = match.Groups[2].Value.Replace("{", "").Replace("}", ""),
                            LineIndex = lineIndex,
                            Level = 0,
                        };

                        result.RootHeader.AddChild(node);
                        continue;
                    }

                    int level = GetHeaderLevel(header);
                    if (level < 0) 
                        continue;

                    if (Regex.IsMatch(header, @"(?:הקדמה)|(?:פתיחה)"))
                        level--;

                    TreeItem current = new TreeItem
                    {
                        Name = match.Groups[2].Value.Replace("{", "").Replace("}", ""),
                        Level = level,  
                        LineIndex = lineIndex,
                    };

                    while (headerStack.Count > 0 && headerStack.Peek().Level >= current.Level)
                        headerStack.Pop();

                    var parent = headerStack.Count > 0 ? headerStack.Peek() : result.RootHeader;
                    parent.AddChild(current);
                    
                    if (parent != result.RootHeader && parent.Level > 1)
                        current.Tags.Add(parent.Name);

                    headerStack.Push(current);
                }
            }

            result.Content = stb.ToString();
            return Task.FromResult(result);
        }

        static int GetHeaderLevel(string tag)
        {
            if (tag == "ot") return 10;
            if (tag.Length == 2 && tag[0] == 'h' && char.IsDigit(tag[1]))
            {
                int level = tag[1] - '0';
                if (level >= 1 && level <= 9) return level;
            }
            return -1; // Not a header
        }
    }
}
