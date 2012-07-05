using System.Collections.Generic;
using GeekStream.Core;
using GeekStream.Core.Queries;
using GeekStream.Core.Views;

namespace GeekStream.Models
{
    public class IndexModel
    {
        public FeedItemView[] RecentItems { get; set; }
        public FeedItemView[] PopularItems { get; set; }
        public FeedView[] PopularFeeds { get; set; }
    }
}