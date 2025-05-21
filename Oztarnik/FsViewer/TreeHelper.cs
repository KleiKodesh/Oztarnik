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
                Name = Path.GetFileNameWithoutExtension(strippedPath),
                Tags = GetTags(strippedPath, rootPath),
            };

            foreach (var dir in Directory.GetDirectories(path))
                item.AddChild(BuildTree(item, dir, rootPath));

            foreach (var file in Directory.GetFiles(path))
            {
                string strippedFilePath = Regex.Replace(file, @"\d+_", "");
                string extension = Path.GetExtension(file).ToLower();
                if (!_validExtensions.Contains(extension))
                    continue;

                item.AddChild(new TreeItem
                {
                    Parent = item,
                    Path = file,
                    Extension = extension,
                    IsFile = true,
                    Name = Path.GetFileNameWithoutExtension(strippedFilePath),
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
