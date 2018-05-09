using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;
using Pillage.Models;

namespace Pillage
{
    internal interface IPersistanceManager
    {
        History GetHistory();
        void SaveToHistory(string folder, string searchText, string filePattern);
    }

    internal class PersistanceManager : IPersistanceManager
    {
        private const string FILENAME = "history.json";

        private string ToJson<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                var jsSerializer = new DataContractJsonSerializer(typeof(T));
                jsSerializer.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private T FromJson<T>(string input)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                var jsSerializer = new DataContractJsonSerializer(typeof(T));
                var obj = (T) jsSerializer.ReadObject(stream);
                return obj;
            }
        }

        public History GetHistory()
        {
            var path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), FILENAME);
            if (!File.Exists(path)) return null;

            var json = File.ReadAllText(path);
            var history = FromJson<History>(json);

            return history;
        }

        public void SaveToHistory(string folder, string searchText, string filePattern)
        {
            History h;

            var path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), FILENAME);

            if (!File.Exists(path))
            {
                h = new History
                {
                    Folders = new List<string> {folder},
                    Searches = new List<string> {searchText},
                    FilePatterns = new List<string> {filePattern}
                };
            }
            else
            {
                var json = File.ReadAllText(path);
                h = FromJson<History>(json);
            }

            InsertOrMoveToTop(h.Searches,searchText);
            InsertOrMoveToTop(h.Folders,folder);
            InsertOrMoveToTop(h.FilePatterns,filePattern);
            
            var j = ToJson(h);
            File.WriteAllText(FILENAME, j);
        }

        private void InsertOrMoveToTop(List<string> l, string x)
        {
            if (l.Contains(x))
            {
                l.Remove(x);
            }
            
            l.Insert(0, x);

            if(l.Count>50) l.RemoveAt(l.Count-1);
        }

    }
}