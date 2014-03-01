using System;
using GeekStream.Core.Domain;
using OrigoDB.Core;

namespace GeekStream.Core.Commands
{
    [Serializable]
    public class AddFeedItemCommand : Command<GeekStreamModel>
    {

        public readonly FeedItem Item;
        public readonly string[] SearchTerms;
        public readonly int FeedId;
        public readonly DateTime Collected;

        public AddFeedItemCommand(FeedItem entry, string[] searchTerms, int feedId, DateTime collected)
        {
            Item = entry;
            SearchTerms = searchTerms;
            FeedId = feedId;
            Collected = collected;
        }

        protected override void Execute(GeekStreamModel model)
        {
            model.AddItem(Item, SearchTerms, FeedId, Collected);
        }
    }
}
