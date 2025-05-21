using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

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
        public static FileContentModel Parse (TreeItem treeItem, bool getContent)
        {
            FileContentModel root = new FileContentModel { TreeItem = treeItem };

            var stb = new StringBuilder();
            Stack<TreeItem> headerStack = new Stack<TreeItem>();            
            int lineIndex = 0;

            if (!File.Exists(treeItem.Path))
                return root;

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

                        root.RootHeader.AddChild(node);
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

                    var parent = headerStack.Count > 0 ? headerStack.Peek() : root.RootHeader;
                    parent.AddChild(current);
                    
                    if (parent != root.RootHeader && parent.Level > 1)
                        current.Tags.Add(parent.Name);

                    headerStack.Push(current);
                }
            }

            root.Content = stb.ToString();
            return root;
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
