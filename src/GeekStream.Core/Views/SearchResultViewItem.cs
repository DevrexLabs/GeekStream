using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeekStream.Core.Domain;

namespace GeekStream.Core.Views
{
    [Serializable]
    public class SearchResultViewItem
    {
        public FeedItemView FeedItem { get; set; }
        public UInt64 Clicks { get; set; }

        public SearchResultViewItem(IndexEntry indexEntry)
        {
            FeedItem = new FeedItemView(indexEntry.Item);
            Clicks = indexEntry.Clicks;
        }
    }
}
