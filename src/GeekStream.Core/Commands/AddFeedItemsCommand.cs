using System;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using LiveDomain.Core;

namespace GeekStream.Core.Commands
{
	[Serializable]
	public class AddFeedItemsCommand : Command<GeekStreamModel>
	{
		public readonly FeedView Feed;
		AddFeedItemCommand[] _items;
		public AddFeedItemsCommand(FeedView feed, AddFeedItemCommand[] items)
		{
			_items = items;
			Feed = feed;
		}

		protected override void Execute(GeekStreamModel model)
		{
			foreach (var item in _items)
			{
				model.AddItem(item.Item, item.SearchTerms, item.FeedId, item.Collected);	
			}
		}
	}
}