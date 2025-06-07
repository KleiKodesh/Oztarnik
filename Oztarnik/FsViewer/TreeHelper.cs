using Oztarnik.FsViewer;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;


namespace Otzarnik.FsViewer
{
    public static class TreeHelper
    {
        static readonly Collection<string> _validExtensions = new Collection<string> { ".txt", ".html", ".pdf" };

        public static TreeItem BuildTree(TreeItem parent, string path, string rootPath)
        {

            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(rootPath))
                throw new ArgumentException("Path and rootPath cannot be null or empty.");
            if (!Directory.Exists(path))
                throw new DirectoryNotFoundException($"The specified path does not exist: {path}");

            string strippedPath = Regex.Replace(path, @"\d+_", "");
            var item = new TreeItem
            {
                Parent = parent,
                Path = path,
                Name = Path.GetFileNameWithoutExtension(strippedPath).Trim(),
                Tags = GetTags(strippedPath, rootPath),
                IsFile = false
            };

            try
            {
                var directories = Directory.GetDirectories(path)
                    .OrderBy(dir =>
                    {
                        var index = Array.IndexOf(FsSort.DirectoryOrder, Path.GetFileName(dir));
                        return index == -1 ? int.MaxValue : index;
                    });

                foreach (var dir in directories)
                {
                    item.AddChild(BuildTree(item, dir, rootPath));
                }


            }
            catch { }


            try
            {
                var files = Directory.GetFiles(path)
                  .Where(file => _validExtensions.Contains(Path.GetExtension(file).ToLower()))
                  .OrderBy(file =>
                  {
                      var index = Array.IndexOf(FsSort.FileOrder, Path.GetFileNameWithoutExtension(file));
                      return index == -1 ? int.MaxValue : index;
                  });

                foreach (var file in files)
                {
                    string strippedFilePath = Regex.Replace(file, @"\d+_", "");
                    item.AddChild(new TreeItem
                    {
                        Parent = item,
                        Path = file,
                        IsFile = true,
                        Extension = Path.GetExtension(file).ToLower(),
                        Name = Path.GetFileNameWithoutExtension(strippedFilePath).Trim(),
                        Tags = GetTags(Path.GetDirectoryName(strippedFilePath), rootPath),
                    });
                }
            }
            catch { }

            return item;
        }

        private static List<string> GetTags(string path, string rootPath)
        {
            if (string.IsNullOrEmpty(path) || string.IsNullOrEmpty(rootPath))
                return null;

            var relativePath = path.Replace(rootPath, "").TrimStart(Path.DirectorySeparatorChar);
            return relativePath.Split(Path.DirectorySeparatorChar).ToList();
        }

    }
}
