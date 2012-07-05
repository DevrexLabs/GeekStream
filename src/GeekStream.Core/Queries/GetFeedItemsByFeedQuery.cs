using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using LiveDomain.Core;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetFeedItemsByFeedQuery : Query<GeekStreamModel, FeedResultView>
    {
        public bool OrderByClicks { get; set; }
        public int PageIndex { get; set; }

        public GetFeedItemsByFeedQuery(int id, int pageIndex)
        {
            Id = id;
            PageIndex = pageIndex;
        }

        public int Id { get; set; }

        #region Overrides of Query<GeekStreamModel,FeedView[]>

        protected override FeedResultView Execute(GeekStreamModel m)
        {
            var skip = PageIndex * 100;
            var result = new FeedResultView();
            var items = m.GetEntriesByFeedId(Id);

            result.TotalResults = m.GetFeedById(Id).Items.Count;

            if (OrderByClicks)
                result.Items = items.OrderByDescending(f => f.Clicks).Skip(skip).Take(100).Select(f => new FeedItemView(f)).ToArray();
            else
                result.Items = items.Skip(skip).Take(100).Select(f => new FeedItemView(f)).ToArray();

            return result;
        }

        #endregion
    }
}
