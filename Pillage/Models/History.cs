using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Pillage.Models
{
    [DataContract]
    internal class History
    {
        [DataMember] public List<string> Searches { get; set; }
        [DataMember] public List<string> Folders { get; set; }
    }
}