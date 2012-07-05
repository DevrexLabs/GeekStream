using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeekStream.Models;

namespace GeekStream.Controllers
{
    public class ApiController : Controller
    {
        //
        // GET: /Api/

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Search(string query, int pageIndex = 0, bool popular = false)
        {
            var before = DateTime.Now;
            var model = ModelFactory.SearchResultModel(query, pageIndex, popular);
            ViewBag.QueryTime = DateTime.Now - before;
            return Json(model,JsonRequestBehavior.AllowGet);
        }
    }
}
