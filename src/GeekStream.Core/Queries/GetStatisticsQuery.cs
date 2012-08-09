using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LiveDomain.Core;
using GeekStream.Core.Domain;

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
