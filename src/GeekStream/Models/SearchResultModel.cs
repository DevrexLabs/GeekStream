using GeekStream.Core.Views;

namespace GeekStream.Models
{
	public class SearchResultModel
    {
        public string Query { get; set; }
	    public int PageIndex { get; set; }
        public FeedItemView[] Results { get; set;}
	    public int Skipped { get; set; }
	    public int TotalResults { get; set; }
    }
}