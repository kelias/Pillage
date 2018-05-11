using System.Collections.Generic;
using System.IO;

namespace Pillage
{
    internal static class FileScanner
    {
        public static List<string> GetAllFiles(string parentFolder, string filePattern,
            Dictionary<string, string> ignoredExtensions, bool searchSubfolders)
        {
            return GetFiles(parentFolder, filePattern, ignoredExtensions, searchSubfolders);
        }

        private static List<string> GetFiles(string parentFolder, string filePattern,
            Dictionary<string, string> ignoredExtensions, bool searchSubfolders)
        {
            var files = new List<string>();

            var allFiles = Directory.GetFiles(parentFolder, filePattern, searchSubfolders? SearchOption.AllDirectories:SearchOption.TopDirectoryOnly);

            foreach (var f in allFiles)
            {
                var ext = Path.GetExtension(f).Replace(".", "").ToLower();

                if (ignoredExtensions.ContainsKey(ext)) continue;

                files.Add(f);
            }
            
            return files;
        }
    }
}