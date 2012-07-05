using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web;
using System.Web.Mvc;
using System.Web.Util;
using GeekStream.Core.Commands;
using GeekStream.Core.Domain;
using GeekStream.Core.Queries;
using GeekStream.Models;
using LiveDomain.Core;

namespace GeekStream.Controllers
{
    public class PageController : Controller
    {
        public ActionResult Index(long id, string searchQuery)
        {
            var query = new GetFeedItemById(id);
            var command = new RegisterClickCommand(id, searchQuery ?? string.Empty);

            MvcApplication.LiveDbClient.Execute(command);

            var model = MvcApplication.LiveDbClient.Execute(query);
            return Redirect(model.Url);
        }
    }
}