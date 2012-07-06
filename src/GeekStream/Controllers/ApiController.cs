using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeekStream.Core;
using GeekStream.Core.Domain;
using GeekStream.Core.Queries;
using GeekStream.Core.Views;
using GeekStream.Models;
using LiveDomain.Core;

namespace GeekStream.Controllers
{
    public class ApiController : Controller
    {

		public ActionResult Search(string query, SortMode sort = SortMode.Recent, int pageIndex = 0)
        {
            var model = ModelFactory.SearchResultModel(query, pageIndex, sort == SortMode.Popular);
            return Json(model.Results.Select(i => i.FeedItem),JsonRequestBehavior.AllowGet);
        }

		public ActionResult Items(SortMode sort)
		{
			var query = new GetItemsQuery(100, sort);
			var items = MvcApplication.LiveDbClient.Execute(query);
			ModelFactory.ReplaceUrl(items);
			return Json(items, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Feeds(SortMode sort)
		{
			var query = new GetFeedsQuery(100,sort);
			var items = MvcApplication.LiveDbClient.Execute(query);
			return Json(items, JsonRequestBehavior.AllowGet);
		}
    }
}
