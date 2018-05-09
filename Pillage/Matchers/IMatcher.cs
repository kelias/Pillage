using System;
using Pillage.Models;

namespace Pillage.Matchers
{
    internal interface IMatcher
    {
        void Search(string file, string searchTerm, Action<SearchResult> resultAction);
    }
}