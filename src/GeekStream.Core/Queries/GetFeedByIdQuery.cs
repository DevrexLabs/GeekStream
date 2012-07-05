using System;
using GeekStream.Core.Domain;
using GeekStream.Core.Views;
using LiveDomain.Core;

namespace GeekStream.Core.Queries
{
    [Serializable]
    public class GetFeedByIdQuery : Query<GeekStreamModel, FeedView>
    {
        public GetFeedByIdQuery(int id)
        {
            Id = id;
        }

        public int Id { get; set; }

        #region Overrides of Query<GeekStreamModel,FeedView[]>

        protected override FeedView Execute(GeekStreamModel m)
        {
            return new FeedView(m.GetFeedById(Id));
        }

        #endregion
    }
}