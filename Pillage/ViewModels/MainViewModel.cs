using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Threading;
using Pillage.Matchers;
using Pillage.Models;
using Pillage.Views;

namespace Pillage.ViewModels
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        private readonly SearchManager searchManager = new SearchManager();
        private readonly MainView view;

        public event PropertyChangedEventHandler PropertyChanged;

        public ICommand SearchCommand { get; set; }
        public ICommand BrowseFoldersCommand { get; set; }

        private readonly ObservableCollection<SearchResult> results = new ObservableCollection<SearchResult>();

        public CollectionViewSource ViewSource { get; set; } = new CollectionViewSource();
        public ObservableCollection<string> RecentSearches { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> RecentFolders { get; set; } = new ObservableCollection<string>();
        public ObservableCollection<string> RecentFilePatterns { get; set; } = new ObservableCollection<string>();

        private string searchText;
        private string filePattern = "*.*";
        private string folder;
        private string status;
        private bool isRunning;
        private SearchResult selectedResult;
        private bool useRegularExpressions;
        private bool searchSubFolders = true;


        public bool SearchSubFolders
        {
            get => searchSubFolders;
            set
            {
                searchSubFolders = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchSubFolders"));
            }
        }

        public bool UseRegularExpressions
        {
            get => useRegularExpressions;
            set
            {
                useRegularExpressions = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("UseRegularExpressions"));
            }
        }

        public SearchResult SelectedResult
        {
            get => selectedResult;
            set
            {
                selectedResult = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SelectedResult"));
                LoadFile();
            }
        }

        public bool IsRunning
        {
            get => isRunning;
            set
            {
                isRunning = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("IsRunning"));
            }
        }

        public string Status
        {
            get => status;
            set
            {
                status = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Status"));
            }
        }

        public string Folder
        {
            get => folder;
            set
            {
                folder = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Folder"));
            }
        }

        public string FilePattern
        {
            get => filePattern;
            set
            {
                if (string.IsNullOrWhiteSpace(value)) value = "*.";

                filePattern = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("FilePattern"));
            }
        }

        public string SearchText
        {
            get => searchText;
            set
            {
                searchText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("SearchText"));
            }
        }

        public MainViewModel(MainView v)
        {
            view = v;

            //ViewSource.GroupDescriptions.Add(new PropertyGroupDescription("Folder"));

            ViewSource.Source = results;
            var h = PersistanceManager.GetHistory();

            if (h != null)
            {
                foreach (var f in h.Folders)
                {
                    RecentFolders.Add(f);
                }

                foreach (var f in h.Searches)
                {
                    RecentSearches.Add(f);
                }

                foreach (var f in h.FilePatterns)
                {
                    RecentFilePatterns.Add(f);
                }

                if (h.Folders.Count > 0) Folder = h.Folders[0];
                if (h.Searches.Count > 0) SearchText = h.Searches[0];
                if (h.FilePatterns.Count > 0) FilePattern = h.FilePatterns[0];
            }

            BindCommands();
        }

        private void BindCommands()
        {
            SearchCommand = new RelayCommand(p => Search());
            BrowseFoldersCommand = new RelayCommand(p => BrowseFolders());
        }

        private void Search()
        {
            if (IsRunning)
            {
                //Cancel
                searchManager.Stop();
                return;
            }

            if (string.IsNullOrWhiteSpace(Folder))
            {
                DisplayMessage("Folder cannot be blank.");
                return;
            }

            if (string.IsNullOrWhiteSpace(FilePattern))
            {
                DisplayMessage("File Pattern cannot be blank.");
                return;
            }

            if (string.IsNullOrWhiteSpace(SearchText))
            {
                DisplayMessage("Search text cannot be blank.");
                return;
            }

            if (!Directory.Exists(Folder))
            {
                DisplayMessage("The folder you selected does not exist.");
                return;
            }

            PersistanceManager.SaveToHistory(Folder,SearchText,FilePattern);

            results.Clear();

            IsRunning = true;
            Status = "Searching...";

            searchManager.IgnoredExtensions = ConfigurationManager.AppSettings["IgnoredExtensions"].Split(',').ToList();
            searchManager.ParentFolder = Folder;
            searchManager.FilePattern = FilePattern;
            searchManager.SearchTerm = SearchText;
            searchManager.SearchSubfolders = SearchSubFolders;
            searchManager.ResultFound = DisplayResult;
            searchManager.SearchComplete = DisplayComplete;
            searchManager.SearchStatusUpdate = DisplayStatusUpdate;

            searchManager.Search(UseRegularExpressions ? (IMatcher) new RegexMatcher() : new GeneralMatcher());
        }

        private void DisplayStatusUpdate(SearchStatus s)
        {
            MoveToUiThread(() => Status = $"Scanning file {s.FilesComplete} of {s.FilesRemaining}...");
        }

        private void DisplayComplete(SearchMetrics metrics)
        {
            IsRunning = false;
            MoveToUiThread(() =>
                Status =
                    $"Search Complete. Elapsed Time: {metrics.ElapsedTime}, Files Searched: {metrics.FilesSearched}");
        }

        private void DisplayResult(SearchResult result)
        {
            MoveToUiThread(() =>
            {
                try
                {
                    result.Icon = IconManager.FindIconForFilename(result.Filename, false);
                    results.Add(result);
                }
                catch
                {
                    //supress
                }
            });
        }

        private void MoveToUiThread(Action a)
        {
            view.Dispatcher.InvokeAsync(a.Invoke, DispatcherPriority.Background);
        }

        private void LoadFile()
        {
            if (SelectedResult == null) return;

            var lineSize = Convert.ToDouble(ConfigurationManager.AppSettings["FileAbstractLineCount"]);

            var beginAt = SelectedResult.LineNumber - lineSize;
            if (beginAt < 0) beginAt = 0;

            var endAt = SelectedResult.LineNumber + lineSize;

            var doc = new FlowDocument {PageWidth = 1000};
            var paragraph = new Paragraph();

            using (var reader = new StreamReader(SelectedResult.FullPath))
            {
                string line;

                double i = 0;

                while ((line = reader.ReadLine()) != null)
                {
                    i++;
                    if (i < beginAt) continue;
                    if (i > endAt) break;

                    if (i == SelectedResult.LineNumber)
                    {
                        var b = new Bold(new Run(line + "\n"));
                        paragraph.Inlines.Add(b);
                    }
                    else
                    {
                        paragraph.Inlines.Add(new Run(line + "\n"));
                    }

                    doc.Blocks.Add(paragraph);
                }

                view.ContentBox.Document = doc;
            }
        }

        private void BrowseFolders()
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.Cancel) return;
                Folder = dialog.SelectedPath;
            }
        }

        private void DisplayMessage(string message)
        {
            MessageBox.Show(message, "Pillage", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        
    }
}