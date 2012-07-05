using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GeekStream.Models;

namespace GeekStream.Controllers
{
    public class SearchController : Controller
    {
        //
        // GET: /Search/
        public ActionResult Index(string query,int pageIndex = 0,bool popular = false)
        {
            var before = DateTime.Now;
			var model = ModelFactory.SearchResultModel(query,pageIndex,popular);
            ViewBag.QueryTime = DateTime.Now - before;

            if(Request.IsAjaxRequest())
                return PartialView(model);

            return View(model);
        }

        //
        // GET: /Search/Details/5

        public ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Search/Create

        public ActionResult Create()
        {
            return View();
        } 

        //
        // POST: /Search/Create

        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
        
        //
        // GET: /Search/Edit/5
 
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Search/Edit/5

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Search/Delete/5
 
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Search/Delete/5

        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here
 
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }
    }
}
