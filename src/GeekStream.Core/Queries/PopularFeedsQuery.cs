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
    public class PopularFeedsQuery : Query<GeekStreamModel,FeedView[]>
    {
        #region Overrides of Query<GeekStreamModel,FeedView[]>

        protected override FeedView[] Execute(GeekStreamModel m)
        {
            var feeds = m.PopularFeeds().ToArray();

            return feeds.Take(6).Select(f => new FeedView(f)).ToArray();
        }

        #endregion
    }
}
