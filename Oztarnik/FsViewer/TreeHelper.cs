using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;


namespace Otzarnik.FsViewer
{
    public static class TreeHelper
    {
        public static TreeItem BuildTree(TreeItem parent, string path, string rootPath)
        {
            string strippedPath = Regex.Replace(path, @"\d+_", "");
            var item = new TreeItem
            {
                Parent = parent,
                Path = path.Replace(rootPath, "").TrimStart('\\'),
                Name = Path.GetFileNameWithoutExtension(strippedPath),
                Tags = GetTags(strippedPath, rootPath),
            };

            foreach (var dir in Directory.GetDirectories(path))
                item.AddChild(BuildTree(item, dir, rootPath));

            foreach (var file in Directory.GetFiles(path))
            {
                string strippedFilePath = Regex.Replace(file, @"\d+_", "");
                item.AddChild(new TreeItem
                {
                    Parent = item,
                    Path = file.Replace(rootPath, "").TrimStart('\\'),
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
