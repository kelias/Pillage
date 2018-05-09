using System;
using System.IO;
using System.Text.RegularExpressions;
using Pillage.Models;

namespace Pillage.Matchers
{
    internal class RegexMatcher : IMatcher
    {
        public void Search(string file, string searchTerm, Action<SearchResult> resultAction)
        {
            var regex = new Regex(searchTerm, RegexOptions.IgnoreCase | RegexOptions.Compiled);

            using (var reader = new StreamReader(file))
            {
                string line;

                double i = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    i++;

                    if (!regex.IsMatch(line)) continue;

                    var s = new SearchResult
                    {
                        FullPath = file,
                        Filename = Path.GetFileName(file),
                        Folder = Path.GetDirectoryName(file),
                        LineNumber = i,
                        SearchTerm = searchTerm
                    };

                    resultAction.Invoke(s);

                    break;
                }
            }
        }
    }
}