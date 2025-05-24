using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HelperConsole
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //var files = Directory.GetFiles("C:\\Users\\Admin\\Desktop\\תורת אמת", "*ללא ניקוד*", SearchOption.AllDirectories);
            //foreach (var file in files)
            //    File.Delete(file);


            var files = Directory.GetFiles(@"C:\Users\Admin\Desktop\Otzarnik\01_תנך", "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                string extension = Path.GetExtension(file);
                string folder = Path.GetDirectoryName(file);
                string foldername = Path.GetFileName(folder);

                string parentName = Regex.Replace(foldername, @"(\d+_)|(חומש)|(מסכת) *", "").Trim();
                if (!fileName.Contains(parentName))
                {
                    string newFileName = $"{fileName} - {parentName}";
                    string newPath = Path.Combine(folder, newFileName + extension);

                    File.Move(file, newPath);
                    Console.WriteLine($"Renamed:\n{file}\n→ {newPath}\n");
                }
            }
        }
    }
}
