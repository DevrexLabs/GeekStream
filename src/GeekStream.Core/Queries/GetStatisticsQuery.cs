using System;
using GeekStream.Core.Domain;
using OrigoDB.Core;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetStatisticsQuery : Query<GeekStreamModel, Statistics>
    {
        public override Statistics Execute(GeekStreamModel model)
        {
            return model.GetStatistics();
        }
    }
}
