using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Windows.Forms;
using Pillage.Models;

namespace Pillage
{
    internal static class PersistanceManager
    {
        private const string FILENAME = "history.json";

        private static string ToJson<T>(T obj)
        {
            using (var stream = new MemoryStream())
            {
                var jsSerializer = new DataContractJsonSerializer(typeof(T));
                jsSerializer.WriteObject(stream, obj);
                return Encoding.UTF8.GetString(stream.ToArray());
            }
        }

        private static T FromJson<T>(string input)
        {
            using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(input)))
            {
                var jsSerializer = new DataContractJsonSerializer(typeof(T));
                var obj = (T) jsSerializer.ReadObject(stream);
                return obj;
            }
        }

        public static History GetHistory()
        {
            var path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), FILENAME);
            if (!File.Exists(path)) return null;

            var json = File.ReadAllText(path);
            var history = FromJson<History>(json);

            return history;
        }

        public static void SaveToHistory(string folder, string searchText)
        {
            History h;

            var path = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), FILENAME);
            if (!File.Exists(path))
            {
                h = new History
                {
                    Folders = new List<string> {folder},
                    Searches = new List<string> {searchText}
                };
            }
            else
            {
                var json = File.ReadAllText(path);
                h = FromJson<History>(json);
            }

            if (!h.Searches.Contains(searchText)) h.Searches.Insert(0, searchText);
            if (!h.Folders.Contains(folder)) h.Folders.Insert(0, folder);

            if (h.Searches.Count > 50) h.Searches.RemoveAt(h.Searches.Count - 1);
            if (h.Folders.Count > 50) h.Folders.RemoveAt(h.Folders.Count - 1);

            var j = ToJson(h);
            File.WriteAllText(FILENAME, j);
        }
    }
}