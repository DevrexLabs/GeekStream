using System;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using OrigoDB.Core;


namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetFeedByIdQuery : Query<GeekStreamModel, FeedView>
    {
        public GetFeedByIdQuery(int id)
        {
            Id = id;
        }

        public readonly int Id;

        public override FeedView Execute(GeekStreamModel m)
        {
            return new FeedView(m.GetFeedById(Id));
        }
    }
}