using GeekStream.Core.Views;

namespace GeekStream.Models
{
    public class FeedModel
    {
        public FeedView Feed { get; set; }
        public FeedItemView[] Items { get; set; }
        public int PageIndex { get; set; }
        public bool SortedByPopular { get; set; }
        public int Skipped { get; set; }
        public int TotalResults { get; set; }
    }
}