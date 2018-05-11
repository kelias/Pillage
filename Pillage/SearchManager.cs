using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Pillage.Matchers;
using Pillage.Models;

namespace Pillage
{
    internal class SearchManager
    {
        public List<string> IgnoredExtensions { get; set; } = new List<string>();

        public string SearchTerm { get; set; }
        public string ParentFolder { get; set; }
        public string FilePattern { get; set; }
        public Action<SearchResult> ResultFound { get; set; }
        public Action<SearchMetrics> SearchComplete { get; set; }
        public Action<SearchStatus> SearchStatusUpdate { get; set; }
        public bool SearchSubfolders { get; set; }

        private CancellationTokenSource cancelSource;

        public void Search(IMatcher matcher)
        {
            cancelSource = new CancellationTokenSource();
            var cancelToken = new CancellationTokenSource().Token;

            var ig = IgnoredExtensions.ToDictionary(k => k, v => v);

            Task.Run(() =>
            {
                var watch = new Stopwatch();
                watch.Start();

                var files = FileScanner.GetFiles(ParentFolder, FilePattern, ig, SearchSubfolders);

                SearchStatusUpdate?.Invoke(new SearchStatus {FilesComplete = 0, FilesRemaining = files.Count});

                var count = 1;
                
                Parallel.ForEach(files,
                    (file, state) =>
                    {
                        if (cancelSource.IsCancellationRequested) state.Break();

                        matcher.Search(file, SearchTerm, ResultFound);

                        //throttle to speed things up
                        if (count++ % 30 == 0)
                            SearchStatusUpdate?.Invoke(
                                new SearchStatus {FilesComplete = count, FilesRemaining = files.Count});
                    });

                SearchStatusUpdate?.Invoke(new SearchStatus {FilesComplete = count, FilesRemaining = files.Count});

                watch.Stop();

                var metrics = new SearchMetrics
                {
                    ElapsedTime = watch.Elapsed,
                    FilesSearched = files.Count
                };

                SearchComplete?.Invoke(metrics);
            }, cancelToken);
        }

        public void Stop()
        {
            cancelSource.Cancel();
        }
    }
}