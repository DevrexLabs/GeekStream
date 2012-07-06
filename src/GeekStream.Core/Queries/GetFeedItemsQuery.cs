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
    public class GetItemsQuery : Query<GeekStreamModel,FeedItemView[]>
    {
    	int _take;
    	SortMode _sortMode;

    	public GetItemsQuery(int take, SortMode sortMode)
    	{
    		_take = take;
    		_sortMode = sortMode;
    	}

        #region Overrides of Query<GeekStreamModel,FeedItemView[]>

        protected override FeedItemView[] Execute(GeekStreamModel m)
        {
			if(_sortMode == SortMode.Popular)
				return m.PopularItems().Take(_take).Select(f => new FeedItemView(f)).ToArray();

			return m.GetMostRecentItems().Take(_take).Select(fi => new FeedItemView(fi)).ToArray();
        }

        #endregion
    }
}
