using System;
using System.Linq;
using GeekStream.Core.Domain;
using LiveDomain.Core;
using GeekStream.Core.Views;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetRecentItemsQuery : Query<GeekStreamModel, FeedItemView[]>
    {
        protected override FeedItemView[] Execute(GeekStreamModel m)
        {
            return m.GetMostRecentItems().Select(fi => new FeedItemView(fi)).Take(5).ToArray();
        }
    }
}