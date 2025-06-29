﻿using Otzarnik.FsViewer;
using Otzarnik.Search;
using Oztarnik.AppData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Otzarnik.FileViewer
{
    public class FileContentModel
    {
        public TreeItem TreeItem { get; set; }
        public string Content { get; set; }
        public HeaderTreeItem RootHeader {get; set;} = new HeaderTreeItem();
    }
    
    public static class ContentParser
    {
        public static Task <FileContentModel> Parse (TreeItem treeItem, bool getContent, ResultModel resultModel)
        {
            FileContentModel result = new FileContentModel { TreeItem = treeItem };

            var stb = new StringBuilder();
            Stack<HeaderTreeItem> headerStack = new Stack<HeaderTreeItem>();            
            int lineIndex = -1;
            int headerIndex = -1;

            if (!File.Exists(treeItem.Path))
                return Task.FromResult(result);

            string content = File.ReadAllText(treeItem.Path);
            
            if (resultModel != null && resultModel.MatchIndex >= 0 && resultModel.MatchIndex + resultModel.MatchValue.Length <= content.Length)
            {
                content = content.Insert(resultModel.MatchIndex + resultModel.MatchValue.Length, "</mark>");
                content = content.Insert(resultModel.MatchIndex, "<mark id=\"match\">");
            }

            foreach (var line in content.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None))
            {
                lineIndex++;

                if (getContent && !string.IsNullOrWhiteSpace(line.Trim()))
                {
                    if (Regex.IsMatch(line, $@"<h\d>(שורה|{Regex.Escape(treeItem.Name)})</h\d>"))
                    {
                        string updatedLine = Regex.Replace(line, @"<h\d>(.*?)</h\d>", "<span>$1</span>");
                        stb.AppendLine($"<div class=\"line\" dir=\"auto\" style=\"display: none;\">{updatedLine}</div>");
                        continue;
                    }

                    else
                        stb.AppendLine($"<div class=\"line\" dir=\"auto\">{line}</div>");
                    
                }
                    

                Match match = Regex.Match(line.TrimStart(), @"^<(h\d+|ot|header).*?>([^\n\r<]+?)</", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    string header = match.Groups[1].Value;

                    if (header == "header")
                    {
                        HeaderTreeItem node = new HeaderTreeItem
                        {
                            Name = match.Groups[2].Value.Replace("{", "").Replace("}", ""),
                            LineIndex = lineIndex,
                            Level = 0,
                            HeaderIndex = headerIndex++,
                        };

                        result.RootHeader.AddChild(node);
                        continue;
                    }

                    int level = GetHeaderLevel(header);
                    if (level < 0) 
                        continue;

                    if (Regex.IsMatch(line, @"(?:הקדמה)|(?:פתיחה)"))
                        level = 6;

                    HeaderTreeItem current = new HeaderTreeItem
                    {
                        Name = match.Groups[2].Value.Replace("{", "").Replace("}", ""),
                        Level = level,  
                        LineIndex = lineIndex,
                        HeaderIndex = headerIndex++,
                    };

                    while (headerStack.Count > 0 && headerStack.Peek().Level >= current.Level)
                        headerStack.Pop();

                    var parent = headerStack.Count > 0 ? headerStack.Peek() : result.RootHeader;
                    parent.AddChild(current);
                    
                    //if (parent != result.RootHeader && 
                    //    parent is HeaderTreeItem headerItem && 
                    //    headerItem.Level > 1)
                            current.Tags.Add(parent.Name);

                    headerStack.Push(current);
                }
            }

            result.Content = stb.ToString()
                .ReplaceShemHashem();

            return Task.FromResult(result);
        }

        static int GetHeaderLevel(string tag)
        {
            if (tag == "ot") return 6;
            if (tag.Length == 2 && tag[0] == 'h' && char.IsDigit(tag[1]))
            {
                int level = tag[1] - '0';
                if (level >= 1 && level <= 6) return level;
            }
            return -1; // Not a header
        }
    }
}
