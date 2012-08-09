using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeekStream.Core.Domain;
using LiveDomain.Core;
using GeekStream.Core.Views;
using System.Text.RegularExpressions;

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
