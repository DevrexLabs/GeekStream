using System;
using System.Linq;
using System.Text;

namespace GeekStream.Core.Domain
{
    [Serializable]
    public class IndexEntry 
    {
        public FeedItem Item { get; set; }
        public readonly string Key;

        public IndexEntry(string key)
        {
            Key = key;
        }
    }
}
