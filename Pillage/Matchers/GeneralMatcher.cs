using System;
using System.Globalization;
using System.IO;
using Pillage.Models;

namespace Pillage.Matchers
{
    internal class GeneralMatcher : IMatcher
    {
        public void Search(string file, string searchTerm, Action<SearchResult> resultAction)
        {
            try
            {
                using (var reader = new StreamReader(file))
                {
                    string line;

                    double i = 0;

                    while ((line = reader.ReadLine()) != null)
                    {
                        i++;

                        if (CultureInfo.CurrentCulture.CompareInfo.IndexOf(line,searchTerm,CompareOptions.IgnoreCase)<0) continue;

                        var s = new SearchResult
                        {
                            FullPath = file,
                            Filename = Path.GetFileName(file),
                            Folder = Path.GetDirectoryName(file),
                            LineNumber = i,
                            SearchTerm = searchTerm
                        };

                        resultAction.Invoke(s);
                    }
                }
            }
            catch 
            {
                //supress
            }
        }
    }
}