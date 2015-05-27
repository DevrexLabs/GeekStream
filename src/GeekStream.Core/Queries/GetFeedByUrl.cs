using System;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using OrigoDB.Core;

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

        public override FeedView Execute(GeekStreamModel m)
        {
            var feed = m.GetFeedByUrl(Url);
            return feed != null ? new FeedView(feed) : null;
        }
    }
}
