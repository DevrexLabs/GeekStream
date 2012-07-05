using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using LiveDomain.Core;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetFeedByUrl : Query<GeekStreamModel, FeedView>
    {

        public readonly string Url;

        public GetFeedByUrl(string url)
        {
            Url = url;
        }

        protected override FeedView Execute(GeekStreamModel m)
        {
            var feed = m.GetFeedByUrl(Url);
            return feed != null ? new FeedView(feed) : null;
        }
    }
}
