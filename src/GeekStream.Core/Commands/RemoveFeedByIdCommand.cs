using System;
using GeekStream.Core.Domain;
using OrigoDB.Core;

namespace GeekStream.Core.Commands
{
    [Serializable]
    public class RemoveFeedByIdCommand : Command<GeekStreamModel>
    {
        /// <summary>
        /// Id of the feed to remove
        /// </summary>
        public readonly int FeedId;


        public RemoveFeedByIdCommand(int feedId)
        {
            FeedId = feedId;
        }

        public override void Execute(GeekStreamModel model)
        {
            if (!model.RemoveFeedById(FeedId))
                throw new CommandAbortedException("No such feed");
        }
    }
}
