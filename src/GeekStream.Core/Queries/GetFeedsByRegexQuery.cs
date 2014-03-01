using System;
using System.Linq;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using OrigoDB.Core;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetFeedsByRegexQuery : Query<GeekStreamModel, FeedView[]>
    {

        public readonly string Pattern;

        public GetFeedsByRegexQuery(string pattern)
        {
            Pattern = pattern;
        }

        protected override FeedView[] Execute(GeekStreamModel model)
        {
            return model.GetFeedsByRegex(Pattern).Take(20).ToArray();
        }
    }
}
