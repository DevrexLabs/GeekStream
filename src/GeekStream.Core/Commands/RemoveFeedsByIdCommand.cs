using System;
using System.Linq;
using GeekStream.Core.Domain;
using OrigoDB.Core;

namespace GeekStream.Core.Commands
{
    [Serializable]
    public class RemoveFeedsByIdCommand : Command<GeekStreamModel,int>
    {
        public readonly int[] FeedsToRemove;

        public RemoveFeedsByIdCommand(params int[] feedIds)
        {
            FeedsToRemove = (int[]) feedIds.Clone();
        }

        public override int Execute(GeekStreamModel model)
        {
            return FeedsToRemove.Count(model.RemoveFeedById);
        }
    }
}