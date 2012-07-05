using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class SearchQuery : Query<GeekStreamModel, SearchResultView>
    {
        public readonly string SearchString;
        public readonly int Skip;
        public readonly int Take;
        public bool OrderByClicks { get; set; }

        public SearchQuery(string searchString, int zeroBasedPageIndex = 0, int pageSize = 100)
        {
            SearchString = searchString;
            Skip = zeroBasedPageIndex * pageSize;
            Take = pageSize;
        }

        protected override SearchResultView Execute(GeekStreamModel m)
        {
            var result = new SearchResultView();
            int totalResults;
            var items = m.Search(SearchString, out totalResults, OrderByClicks);
            var queryResults = items.Skip(Skip).Take(Take).Select(indexEntry => new SearchResultViewItem(indexEntry)).ToArray();

            result.Query = SearchString;
            result.TotalResults = totalResults;
            result.Items = queryResults;
            return result;

        }
    }

    
}
