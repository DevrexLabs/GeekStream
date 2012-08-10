using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;
using GeekStream.Core.Domain;

namespace GeekStream.Core.Commands
{
    [Serializable]
    public class SetFeedLastCollectedCommand : Command<GeekStreamModel>
    {

        public readonly int FeedId;
        public readonly DateTime When;

        public SetFeedLastCollectedCommand(int feedId, DateTime when)
        {
            FeedId = feedId;
            When = when;
        }

        protected override void Execute(GeekStreamModel model)
        {
            try
            {
                model.SetFeedLastCollected(FeedId, When);
            }
            catch (ArgumentException e)
            {
                
                throw new CommandAbortedException(e.Message);
            } 

        }
    }
}
