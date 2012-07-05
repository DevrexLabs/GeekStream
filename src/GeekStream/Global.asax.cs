using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GeekStream.Core.Domain;
using LiveDomain.Core;
using LiveDomain.Enterprise;

namespace GeekStream
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "ApiRoute", // Route name
                "Api/{action}/{id}", // URL with parameters
                new {controller = "Api", action = "Index", id = UrlParameter.Optional} // Parameter defaults
                );

            routes.MapRoute(
                "DefaultSlim", // Route name
                "Home/{action}", // URL with parameters
                new {controller = "Home", action = "Index"} // Parameter defaults
                );

            routes.MapRoute(
                "DefaultMedium", // Route name
                "{controller}/{id}", // URL with parameters
                new {controller = "Home", action = "Index", id = UrlParameter.Optional} // Parameter defaults
                );

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new {controller = "Home", action = "Index", id = UrlParameter.Optional} // Parameter defaults
                );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);


            //WARN: The following code will become obsolete in 
            //version 0.3 of the enterprise client lib
            string liveDbConnectionString = ConfigurationManager.ConnectionStrings["geekstream"].ConnectionString;
            _liveDbConnectionSettings = LiveDbConnectionSettings.Parse(liveDbConnectionString);

        }


        //WARN: The following code will become obsolete in 
        //version 0.3 of the enterprise client lib
        private static LiveDbConnectionSettings _liveDbConnectionSettings;


        //WARN: The following code will become obsolete in 
        //version 0.3 of the enterprise client lib
        public static ITransactionHandler<GeekStreamModel> LiveDbClient
        {
            get
            {
                return _liveDbConnectionSettings.GetClient<GeekStreamModel>();
            }
        }
    }
}