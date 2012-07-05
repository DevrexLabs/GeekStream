using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeekStream.Models;

namespace GeekStream.Controllers
{
    public class FeedController : Controller
    {
        //
        // GET: /Feed/

        public ActionResult Index(int id, int pageIndex=0, bool popular = false)
        {
            var before = DateTime.Now;
            var model = ModelFactory.FeedModel(id, pageIndex, popular);
            ViewBag.QueryTime = DateTime.Now - before;

            if(Request.IsAjaxRequest())
                return PartialView(model);
            return View(model);
        }

    }
}
