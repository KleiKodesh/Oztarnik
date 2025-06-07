using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oztarnik.Helpers
{
    public static class PathExtensions
    {
        public static IEnumerable<string> SafeGetDirectories(string path)
        {
            try
            {
                return Directory.GetDirectories(path);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.Error.WriteLine($"Access denied to directory: {path}. Skipping. {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error accessing directory: {path}. Skipping. {ex.Message}");
            }
            return Enumerable.Empty<string>();
        }

        public static IEnumerable<string> SafeGetFiles(string path)
        {
            try
            {
                return Directory.GetFiles(path);
            }
            catch (UnauthorizedAccessException ex)
            {
                Console.Error.WriteLine($"Access denied to files in directory: {path}. Skipping. {ex.Message}");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error accessing files in directory: {path}. Skipping. {ex.Message}");
            }
            return Enumerable.Empty<string>();
        }
    }
}
