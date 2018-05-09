using Pillage.Models;

namespace Pillage
{
    internal interface IPersistanceManager
    {
        History GetHistory();
        void SaveToHistory(string folder, string searchText, string filePattern);
    }
}