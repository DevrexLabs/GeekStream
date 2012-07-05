using System.Collections.Generic;
using GeekStream.Core;
using GeekStream.Core.Queries;
using GeekStream.Core.Views;

namespace GeekStream.Models
{
	public class SearchResultModel
    {
        public string Query { get; set; }
	    public int PageIndex { get; set; }
        public SearchResultViewItem[] Results { get; set;}
	    public bool SortedByPopular { get; set; }
	    public int Skipped { get; set; }
	    public int TotalResults { get; set; }
    }
}