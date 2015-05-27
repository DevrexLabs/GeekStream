using System;

namespace GeekStream.Core.Domain
{
    [Serializable]
    public class Statistics
    {
        public long TotalFeeds { get; set; }
        public long TotalFeedItems { get; set; }
        public long UniqueKeywords { get; set; }
        public long TotalKeywords { get; set; }
    }
}
