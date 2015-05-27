using System;
using System.Linq;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using OrigoDB.Core;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetFeedItemsByFeedQuery : Query<GeekStreamModel, FeedResultView>
    {
        public const int ItemsPerPage = 30;
        public readonly int PageIndex;
        public readonly int Id;

        public GetFeedItemsByFeedQuery(int id, int pageIndex)
        {
            Id = id;
            PageIndex = pageIndex;
        }

        public override FeedResultView Execute(GeekStreamModel m)
        {
            var skip = PageIndex * ItemsPerPage;
            var result = new FeedResultView();
            var items = m.GetEntriesByFeedId(Id);

            result.TotalResults = m.GetFeedById(Id).Items.Count;
            result.Items = items
                .OrderByDescending(item => item.Published)
                .Skip(skip)
                .Take(ItemsPerPage)
                .Select(f => new FeedItemView(f)).ToArray();
            return result;
        }
    }
}
