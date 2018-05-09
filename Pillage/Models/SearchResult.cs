using System.Windows.Media;

namespace Pillage.Models
{
    internal class SearchResult
    {
        public string Folder { get; set; }
        public string FullPath { get; set; }
        public string Filename { get; set; }
        public string SearchTerm { get; set; }
        public double LineNumber { get; set; }
        public ImageSource Icon { get; set; }
    }
}