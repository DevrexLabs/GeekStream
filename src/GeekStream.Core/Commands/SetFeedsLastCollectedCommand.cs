using System;
using System.Collections.Generic;
using GeekStream.Core.Domain;
using LiveDomain.Core;

namespace GeekStream.Core.Commands
{
	[Serializable]
	public class SetFeedsLastCollectedCommand : Command<GeekStreamModel>
	{
		Dictionary<int, DateTime> _feeds;

		public SetFeedsLastCollectedCommand(Dictionary<int, DateTime> feeds)
		{
			_feeds = feeds;
		}

		protected override void Execute(GeekStreamModel model)
		{
			foreach (KeyValuePair<int, DateTime> feedInfo in _feeds)
			{
				try
				{
					model.SetFeedLastCollected(feedInfo.Key, feedInfo.Value);
				}
				catch (ArgumentException e)
				{

					//throw new CommandAbortedException(e.Message);
				}
			}
		}
	}
}