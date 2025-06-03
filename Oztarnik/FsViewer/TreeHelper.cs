using Oztarnik.FsViewer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace Otzarnik.FsViewer
{
    public static class TreeHelper
    {
        static readonly Collection<string> _validExtensions = new Collection<string> { ".txt", ".html", ".pdf"};
       
        public static TreeItem BuildTree(TreeItem parent, string path, string rootPath)
        {
            string strippedPath = Regex.Replace(path, @"\d+_", "");
            var item = new TreeItem
            {
                Parent = parent,
                Path = path,
                Name = Path.GetFileNameWithoutExtension(strippedPath).Trim(),
                Tags = GetTags(strippedPath, rootPath),
            };

            var directories = Directory.GetDirectories(path)
                .OrderBy(dir =>
                {
                    var index = Array.IndexOf(FsSort.DirectoryOrder, Path.GetFileName(dir));
                    return index == -1 ? int.MaxValue : index; // Unmatched items go to the end
                });

            foreach (var dir in directories)
                item.AddChild(BuildTree(item, dir, rootPath));

            var files = Directory.GetFiles(path)
                .OrderBy(dir =>
                {
                    var index = Array.IndexOf(FsSort.FileOrder, Path.GetFileNameWithoutExtension(dir));
                    return index == -1 ? int.MaxValue : index; // Unmatched items go to the end
                });

            foreach (var file in files)
            {
                string strippedFilePath = Regex.Replace(file, @"\d+_", "");
                string extension = Path.GetExtension(file).ToLower();
                if (!_validExtensions.Contains(extension))
                    continue;

                item.AddChild(new TreeItem
                {
                    Parent = item,
                    Path = file,
                    IsFile = true,
                    Extension = extension,
                    Name = Path.GetFileNameWithoutExtension(strippedFilePath).Trim(),
                    Tags = GetTags(Path.GetDirectoryName(strippedFilePath), rootPath),
                });
            }

            return item;
        }

        private static List<string> GetTags(string path, string rootPath)
        {
            path = path.Replace(rootPath, "").TrimStart(Path.DirectorySeparatorChar);
            var dirs = path.Split(Path.DirectorySeparatorChar);
            return dirs.Length > 1 ? dirs.ToList() : null;
        }
    }
}
