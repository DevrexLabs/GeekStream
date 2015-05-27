using System;
using System.Linq;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using OrigoDB.Core;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetFeedsQuery : Query<GeekStreamModel,FeedView[]>
    {
    	public readonly int Take;

    	public GetFeedsQuery(int take)
    	{
    		Take = take;
    	}

        public override FeedView[] Execute(GeekStreamModel model)
        {
            return model.MostRecentlyModifiedFeeds()
                .Take(Take)
                .Select(f => new FeedView(f)).ToArray();
        }
    }
}
