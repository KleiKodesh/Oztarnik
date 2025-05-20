using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelperConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var files = Directory.GetFiles("C:\\Users\\Admin\\Desktop\\תורת אמת", "*ללא ניקוד*", SearchOption.AllDirectories);
            foreach (var file in files)
                File.Delete(file);
        }
    }
}
