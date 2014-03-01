using System;
using GeekStream.Core.Domain;
using OrigoDB.Core;

namespace GeekStream.Core.Commands
{
    [Serializable]
    public class SetFeedsLastCollectedCommand : Command<GeekStreamModel>
    {

        public readonly int[] FeedIds;
        public readonly DateTime When;

        public SetFeedsLastCollectedCommand(int[] feedIds, DateTime when)
        {
            FeedIds = feedIds;
            When = when;
        }

        protected override void Execute(GeekStreamModel model)
        {
            try
            {
                model.SetFeedsLastCollected(FeedIds, When);
            }
            catch (ArgumentException e)
            {
                
                throw new CommandAbortedException(e.Message);
            } 

        }
    }
}
