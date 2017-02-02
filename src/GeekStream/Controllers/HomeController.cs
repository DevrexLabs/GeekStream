using System;
using System.Web.Mvc;
using GeekStream.Core.Queries;
using GeekStream.Models;

namespace GeekStream.Controllers
{
    public class HomeController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            return View(MvcApplication.DbClient.Execute(new GetStatisticsQuery()));
        }

        public ActionResult Api()
        {
            return View();
        }

		public ActionResult Feed(int id, int pageIndex = 0)
		{
			var before = DateTime.Now;
			var model = ModelFactory.FeedModel(id, pageIndex);
			ViewBag.QueryTime = DateTime.Now - before;

			if (Request.IsAjaxRequest()) return PartialView(model);
			return View(model);
		}

		public ActionResult Page(long id)
		{
			var query = new GetFeedItemById(id);
			var model = MvcApplication.DbClient.Execute(query);
			return Redirect(model.Url);
		}

		public ActionResult Search(string query, int pageIndex = 0)
		{
			var before = DateTime.Now;
			var model = ModelFactory.SearchResultModel(query, pageIndex);
			ViewBag.QueryTime = DateTime.Now - before;

			if (Request.IsAjaxRequest())
				return PartialView(model);

			return View(model);
		}


        [ChildActionOnly]
        public ActionResult Footer()
        {
            return PartialView("_footer", ModelFactory.IndexModel());
        }
    }
}
