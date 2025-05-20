using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Oztarnik.Main
{
    /// <summary>
    /// Interaction logic for OtzarnikView.xaml
    /// </summary>
    public partial class OtzarnikView : UserControl
    {
        public OtzarnikView()
        {
            InitializeComponent();
            var list = new List<string>
            {
                "C:\\Users\\Admin\\Desktop\\תורת אמת",
                "C:\\אוצריא\\אוצריא"
            };

            fsViewer.SourceCollection = list;
        }
    }
}
