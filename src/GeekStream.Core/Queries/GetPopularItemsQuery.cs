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
    public class GetPopularItemsQuery : Query<GeekStreamModel,FeedItemView[]>
    {
        #region Overrides of Query<GeekStreamModel,FeedItemView[]>

        protected override FeedItemView[] Execute(GeekStreamModel m)
        {
            return m.PopularItems().Take(5).Select(f => new FeedItemView(f)).ToArray();
        }

        #endregion
    }
}
