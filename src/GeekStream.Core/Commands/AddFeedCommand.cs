using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;
using GeekStream.Core.Domain;

namespace GeekStream.Core.Commands
{
    [Serializable]
    public class AddFeedCommand : CommandWithResult<GeekStreamModel, int>
    {

        public readonly Feed Feed;


        public AddFeedCommand(Feed feed)
        {
            Feed = feed;
        }

        protected override int Execute(GeekStreamModel model)
        {
            try
            {
                model.AddFeed(Feed);
                return Feed.LongId;
            }
            catch (Exception ex)
            {
                throw new CommandFailedException(ex.Message);
            }
        }
    }
}
