using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeekStream.Core;
using GeekStream.Core.Queries;
using GeekStream.Core.Views;

namespace GeekStream.Models
{
    public static class ModelFactory
    {
		public static SearchResultModel SearchResultModel(string query,int pageIndex, bool popular)
        {
			var searchQuery = new SearchQuery(query,pageIndex) {OrderByClicks = popular};
            var model = new SearchResultModel();
            var queryResults = MvcApplication.LiveDbClient.Execute(searchQuery);
		    model.Results = queryResults.Items;
			model.Query = queryResults.Query;
            model.TotalResults = queryResults.TotalResults;
		    model.SortedByPopular = popular;
		    model.PageIndex = pageIndex;
		    model.Skipped = (pageIndex*100);

			ReplaceUrl(model.Results.Select(i => i.FeedItem));

            return model;
        }

        public static IndexModel IndexModel()
        {
            var model = new IndexModel();
			model.RecentItems = MvcApplication.LiveDbClient.Execute(new GetItemsQuery(5, SortMode.Popular));
            model.PopularFeeds = MvcApplication.LiveDbClient.Execute(new GetFeedsQuery(6,SortMode.Popular));
            model.PopularItems = MvcApplication.LiveDbClient.Execute(new GetItemsQuery(5,SortMode.Popular));

			ReplaceUrl(model.RecentItems);
			ReplaceUrl(model.PopularItems);


            
            return model;
        }

        public static FeedModel FeedModel(int id,int pageIndex, bool popular)
        {
            var model = new FeedModel();
            var result = MvcApplication.LiveDbClient.Execute(new GetFeedItemsByFeedQuery(id, pageIndex) { OrderByClicks = popular });
            model.Items = result.Items;
            model.Feed = MvcApplication.LiveDbClient.Execute(new GetFeedByIdQuery(id));
            model.TotalResults = result.TotalResults;
            model.SortedByPopular = popular;
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