using System;
using System.Linq;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using OrigoDB.Core;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetItemsQuery : Query<GeekStreamModel,FeedItemView[]>
    {
    	public readonly int Take;

     	public GetItemsQuery(int take)
    	{
    		Take = take;
    	}

        protected override FeedItemView[] Execute(GeekStreamModel m)
        {
			return m.GetMostRecentItems()
                .Take(Take)
                .Select(fi => new FeedItemView(fi))
                .ToArray();
        }
    }
}
