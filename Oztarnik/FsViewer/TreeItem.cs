using System.Collections.Generic;
using WpfLib.ViewModels;

namespace Otzarnik.FsViewer
{
    public class TreeItem : CheckedTreeItemBase<TreeItem>
    {
        string _extension;
        public string Path { get; set; }
        public bool IsFile { get; set; }
        public string Extension { get => _extension ?? System.IO.Path.GetExtension(Path).ToLower(); set => _extension = value.ToLower(); }
        public List<string> Tags { get; set; } = new List<string>();

        public override string ToString() => Name ?? System.IO.Path.GetFileNameWithoutExtension(Path);

    }

    public class HeaderTreeItem : TreeItem
    {
        public int HeaderIndex { get; set; }  
        public int LineIndex { get; set; }
        public int Level { get; set; }
    }
}
