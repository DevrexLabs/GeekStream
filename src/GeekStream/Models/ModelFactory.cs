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

		    var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
		    foreach (var item in model.Results) ReplaceUrl(urlHelper, item.FeedItem);
		    
            return model;
        }

        public static IndexModel IndexModel()
        {
            var model = new IndexModel();
            model.RecentItems = MvcApplication.LiveDbClient.Execute(new GetRecentItemsQuery());
            model.PopularFeeds = MvcApplication.LiveDbClient.Execute(new PopularFeedsQuery());
            model.PopularItems = MvcApplication.LiveDbClient.Execute(new GetPopularItemsQuery());

            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            foreach (var item in model.PopularItems) ReplaceUrl(urlHelper,item);
            foreach (var item in model.RecentItems) ReplaceUrl(urlHelper,item);
            
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

            var urlHelper = new UrlHelper(HttpContext.Current.Request.RequestContext);
            foreach (var item in model.Items) ReplaceUrl(urlHelper, item);

            return model;
        }

        private static void ReplaceUrl(UrlHelper urlHelper, FeedItemView item)
        {
            item.Url = urlHelper.Action("Index", "Page", new { id = item.Id }, HttpContext.Current.Request.IsSecureConnection ? "https" : "http");
        }
    }
}