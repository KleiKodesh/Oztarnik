using System;
using System.CodeDom.Compiler;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WpfLib.ViewModels;

namespace Otzarnik.FsViewer
{
    internal class HeadersListView : FsListView
    {
        public RelayCommand<TreeItem> NavigateCommand => new RelayCommand<TreeItem>(item => Navigate(item));
        public virtual void Navigate(TreeItem item)
        {
            if (item != null)
                OnNavigationRequested(item);
        }

        public override void Goto(TreeItem item)
        {
            if (item != null)
                CurrentItem = item;
        }
    }
}
