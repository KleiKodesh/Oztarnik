using System.Collections.Generic;
using WpfLib.ViewModels;

namespace Otzarnik.FsViewer
{

    public class TreeItem : CheckedTreeItemBase<TreeItem>
    {
        public string Path { get; set; }
        public bool IsFile { get; set; }
        public int LineIndex { get; set; }
        public int Level { get; set; }
        public List<string> Tags { get; set; } = new List<string>();

        public override string ToString() => Name ?? System.IO.Path.GetFileNameWithoutExtension(Path);

    }
}
