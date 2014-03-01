using System;
using GeekStream.Core.Domain;
using OrigoDB.Core;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetStatisticsQuery : Query<GeekStreamModel, Statistics>
    {
        protected override Statistics Execute(GeekStreamModel model)
        {
            return model.GetStatistics();
        }
    }
}
