using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeekStream.Core.Queries;
using GeekStream.Models;

namespace GeekStream.Controllers
{
    public class HomeController : Controller
    {
        //
        // GET: /Home/
        public ActionResult Index()
        {
            return View();
        }
        //
        // GET: /Home/About/5
        public ActionResult About()
        {
            return View();
        }

        [ChildActionOnly]
        public ActionResult Footer()
        {
            return PartialView("_footer",ModelFactory.IndexModel());
        }
    }
}
