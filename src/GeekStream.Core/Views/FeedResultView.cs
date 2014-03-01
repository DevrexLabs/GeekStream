using System;

namespace GeekStream.Core.Views
{
    [Serializable]
    public class FeedResultView
    {
        public FeedItemView[] Items { get; set; }
        public int TotalResults { get; set; }
    }
}