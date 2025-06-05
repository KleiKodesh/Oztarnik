using System.Collections.Generic;
using WpfLib.ViewModels;

namespace Otzarnik.FsViewer
{
    public class TreeItem : CheckedTreeItemBase<TreeItem>
    {
        string _extension;
        public string Path { get; set; }
        public bool? IsFile { get; set; }
        public string Extension
        {
            get => _extension ?? (Path != null ? System.IO.Path.GetExtension(Path).ToLower() : string.Empty);
            set => _extension = value.ToLower();
        }

        public List<string> Tags { get; set; } = new List<string>();

        public override string ToString() => Name ?? System.IO.Path.GetFileNameWithoutExtension(Path);

        public TreeItem Clone()
        {
            var clone = new TreeItem
            {
                Parent = this.Parent,
                Path = this.Path,
                IsFile = this.IsFile,
                Extension = this._extension, // Copy backing field to preserve original
                Name = this.Name,
                IsChecked = this.IsChecked,
                Tags = new List<string>(this.Tags)
            };

            return clone;
        }

    }

    public class HeaderTreeItem : TreeItem
    {
        public int HeaderIndex { get; set; }  
        public int LineIndex { get; set; }
        public int Level { get; set; }

        public TreeItem GetParent()
        {
            var parent = this.Parent;
            while (parent.IsFile == null && parent.Parent != null)
                parent = parent.Parent;
            return parent;
        }

    }
}
