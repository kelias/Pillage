using System.Collections.Generic;
using System.IO;

namespace Pillage
{
    internal static class FileIndexer
    {
        public static List<string> GetAllFiles(string parentFolder, string filePattern,
            Dictionary<string, string> ignoredExtensions, bool searchSubfolders)
        {
            return GetFiles(parentFolder, filePattern, ignoredExtensions, searchSubfolders);
        }

        private static List<string> GetFiles(string parentFolder, string filePattern,
            Dictionary<string, string> ignoredExtensions, bool searchSubfolders)
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
                    files.AddRange(GetFiles(f, filePattern, ignoredExtensions, true));
                }
            }

            try
            {
                var allFiles = Directory.GetFiles(parentFolder, filePattern);

                foreach (var f in allFiles)
                {
                    var ext = Path.GetExtension(f).Replace(".", "").ToLower();

                    if (ignoredExtensions.ContainsKey(ext)) continue;

                    files.Add(f);
                }
            }
            catch
            {
                //no access to folder so skip
            }

            return files;
        }
    }
}