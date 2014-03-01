using System;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using OrigoDB.Core;

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

        protected override FeedItemView Execute(GeekStreamModel m)
        {
            return new FeedItemView(m.GetItemByLongId(Id));
        }

    }
}
