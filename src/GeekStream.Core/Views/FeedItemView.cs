using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GeekStream.Core.Views
{
    [Serializable]
    public class FeedItemView
    {
        public Int64 Id { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
        public string FeedTitle { get; set; }
        public int Clicks { get; set; }
        public DateTimeOffset Published { get; set; }
        public int FeedId { get; set; }

        public FeedItemView(FeedItem item)
        {
            Id = item.LongId;
            Title = item.Title;
            Summary = item.Summary;
            Url = item.Url;
            FeedTitle = item.Feed.Title;
            FeedId = item.Feed.Id;
            Clicks = item.Clicks;
            Published = item.Published;
        }

        
    }
}
