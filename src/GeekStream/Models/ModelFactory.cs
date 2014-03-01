using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using GeekStream.Core.Queries;
using GeekStream.Core.Views;

namespace GeekStream.Models
{
    public static class ModelFactory
    {
		public static SearchResultModel SearchResultModel(string query,int pageIndex)
		{
			var searchQuery = new SearchQuery(query,pageIndex);
            var model = new SearchResultModel();
            var queryResults = MvcApplication.DbClient.Execute(searchQuery);
		    model.Results = queryResults.Items;
			model.Query = queryResults.Query;
            model.TotalResults = queryResults.TotalResults;
		    model.PageIndex = pageIndex;
		    model.Skipped = (pageIndex*100);

			ReplaceUrl(model.Results);

            return model;
        }

        public static IndexModel IndexModel()
        {
            return new IndexModel
            {
                Statistics = MvcApplication.DbProxy.GetStatistics()
            };
        }

        public static FeedPageModel FeedModel(int id,int pageIndex)
        {
            var model = new FeedPageModel();
            var result = MvcApplication.DbClient.Execute(new GetFeedItemsByFeedQuery(id, pageIndex));
            model.Items = result.Items;
            model.Feed = MvcApplication.DbClient.Execute(new GetFeedByIdQuery(id));
            model.TotalResults = result.TotalResults;
            model.PageIndex = pageIndex;
            model.Skipped = (pageIndex*100);
			ReplaceUrl(model.Items);
            return model;
        }

    	public static void ReplaceUrl(IEnumerable<FeedItemView> items )
		{
			var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);

    		foreach (var item in items)
    		{
				item.Url = urlHelper.Action("Page", "Home", new { id = item.Id }, HttpContext.Current.Request.IsSecureConnection ? "https" : "http");	
    		}
		}
    }
}