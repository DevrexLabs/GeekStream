using System;
using GeekStream.Core.Domain;
using LiveDomain.Core;

namespace GeekStream.Core.Commands
{
	[Serializable]
	public class AddFeedItemsCommand : Command<GeekStreamModel>
	{

		AddFeedItemCommand[] _items;
		public AddFeedItemsCommand(AddFeedItemCommand[] items)
		{
			_items = items;
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