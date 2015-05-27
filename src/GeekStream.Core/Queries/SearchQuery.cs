using System;
using System.Linq;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using OrigoDB.Core;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class SearchQuery : Query<GeekStreamModel, SearchResultView>
    {
        public readonly string SearchString;
        public readonly int Skip;
        public readonly int Take;

        public SearchQuery(string searchString, int zeroBasedPageIndex = 0, int pageSize = 30)
        {
            SearchString = searchString;
            Skip = zeroBasedPageIndex * pageSize;
            Take = pageSize;
        }

        public override SearchResultView Execute(GeekStreamModel model)
        {
            var result = new SearchResultView();
            int totalResults;
            var items = model.Search(SearchString, out totalResults);
            var queryResults = items
                .OrderByDescending(item => item.Published)
                .Skip(Skip)
                .Take(Take)
                .Select(feedItem => new FeedItemView(feedItem))
                .ToArray();

            result.Query = SearchString;
            result.TotalResults = totalResults;
            result.Items = queryResults;
            return result;

        }
    }

    
}
