using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Pillage
{
    internal static class FileScanner
    {
        public static List<string> GetAllFiles(string parentFolder, string filePattern, List<string> ignoredExtensions, bool searchSubfolders)
        {
            return GetFiles(parentFolder, filePattern, ignoredExtensions, searchSubfolders);
        }

        private static List<string> GetFiles(string parentFolder, string filePattern, List<string> ignoredExtensions, bool searchSubfolders)
        {
            string[] folders;

            try
            {
                folders = Directory.GetDirectories(parentFolder);
            }
            catch
            {
                //locked files will hit this, so skip em
                folders = null;
            }

            var files = new List<string>();

            if (folders?.Length > 0 && searchSubfolders)
            {
                foreach (var f in folders)
                {
                    //Recurse each folder
                    files.AddRange(GetFiles(f, filePattern, ignoredExtensions,true));
                }
            }

            var allFiles = Directory.GetFiles(parentFolder, filePattern);

            foreach (var f in allFiles)
            {
                var ext = Path.GetExtension(f).Replace(".", "").ToLower();

                if (ignoredExtensions.Any(w => w == ext)) continue;
                files.Add(f);
            }
            

            return files;
        }
    }
}