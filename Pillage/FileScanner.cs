using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pillage
{
    internal static class FileScanner
    {
        public static List<string> GetFiles(string parentFolder, string filePattern,
            Dictionary<string, string> ignoredExtensions, bool searchSubfolders)
        {
            var allFiles = Directory.GetFiles(parentFolder, filePattern,
                searchSubfolders ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);

            var files = (from f in allFiles
                let ext = Path.GetExtension(f).Replace(".", "").ToLower()
                where !ignoredExtensions.ContainsKey(ext)
                select f).ToList();

            return files;
        }
    }
}