using System.Linq;
using System.Web.Mvc;
using GeekStream.Core;
using GeekStream.Core.Queries;
using GeekStream.Models;

namespace GeekStream.Controllers
{
    public class ApiController : Controller
    {

		public ActionResult Search(string query, int pageIndex = 0)
        {
            var model = ModelFactory.SearchResultModel(query, pageIndex);
            return Json(model.Results,JsonRequestBehavior.AllowGet);
        }

		public ActionResult Items()
		{
			var query = new GetItemsQuery(100);
			var items = MvcApplication.DbClient.Execute(query);
			ModelFactory.ReplaceUrl(items);
			return Json(items, JsonRequestBehavior.AllowGet);
		}

		public ActionResult Feeds()
		{
			var query = new GetFeedsQuery(100);
			var items = MvcApplication.DbClient.Execute(query);
			return Json(items, JsonRequestBehavior.AllowGet);
		}
    }
}
