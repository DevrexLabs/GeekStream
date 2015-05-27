using System;
using GeekStream.Core.Domain;
using OrigoDB.Core;

namespace GeekStream.Core.Commands
{
    [Serializable]
    public class AddFeedCommand : Command<GeekStreamModel, int>
    {

        public readonly Feed Feed;


        public AddFeedCommand(Feed feed)
        {
            Feed = feed;
        }

        public override int Execute(GeekStreamModel model)
        {
            try
            {
                return model.AddFeed(Feed);
            }
            catch (Exception ex)
            {
                throw new CommandAbortedException(ex.Message);
            }
        }
    }
}
