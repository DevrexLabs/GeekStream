using System;
using System.Collections.Generic;
using System.Linq;
using GeekStream.Core.Domain;

namespace GeekStream.Core.Views
{
    [Serializable]
    public class FeedView
    {
        public int LongId { get; set; }
        public string Url { get; set; }
        public string Title { get; set; }
        public readonly HashSet<string> ItemUrls;
        public string Description { get; set; }
        public int Id { get; set; }

        public FeedView(Feed feed)
        {
            Id = feed.Id;
            LongId = feed.LongId;
            Url = feed.Url;
            Title = feed.Title;
            ItemUrls = new HashSet<string>(feed.Items.Select(f => f.Url));
            Description = feed.Description;
        }
    }
}
