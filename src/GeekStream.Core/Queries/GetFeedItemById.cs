using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeekStream.Core.Domain;
using LiveDomain.Core;
using GeekStream.Core.Views;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetFeedItemById: Query<GeekStreamModel,FeedItemView>
    {
        public readonly Int64 Id;

        public GetFeedItemById(Int64 id)
        {
            Id = id;
        }

        #region Overrides of Query<BlogModel,BlogEntryView>

        protected override FeedItemView Execute(GeekStreamModel m)
        {
            return new FeedItemView(m.GetItemByLongId(Id));
        }

        #endregion
    }
}
