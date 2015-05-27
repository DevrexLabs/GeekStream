using System;
using GeekStream.Core.Domain;
using OrigoDB.Core;

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

        public override void Execute(GeekStreamModel model)
        {
            if (!model.RemoveFeedByUrl(Url))
                throw new CommandAbortedException("No such feed");
        }

        
    }
}
