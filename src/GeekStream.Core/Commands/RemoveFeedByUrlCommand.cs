using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeekStream.Core.Domain;
using LiveDomain.Core;

namespace GeekStream.Core.Commands
{
    [Serializable]
    public class RemoveFeedByUrlCommand : Command<GeekStreamModel>
    {
        public readonly string Url;

        public RemoveFeedByUrlCommand(string url)
        {
            Url = url;
        }

        protected override void Execute(GeekStreamModel model)
        {
            if (!model.RemoveFeedByUrl(Url))
                throw new CommandFailedException("No such feed");
        }

        
    }
}
