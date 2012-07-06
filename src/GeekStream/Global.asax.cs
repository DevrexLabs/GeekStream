using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GeekStream.Core;
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

			RegisterApiRoutes(routes);

			routes.MapRoute("Default", // Route name
				"{action}/{id}", // URL with parameters
				new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
				);
		}

		static void RegisterApiRoutes(RouteCollection routes)
		{
			routes.MapRoute("ApiRoute", // Route name
					"Api/V1/{sort}", // URL with parameters
					new { controller = "Api", action = "Items"} // Parameter defaults
					);

			routes.MapRoute("ApiSearchRoute", // Route name
				"Api/V1/Search/{query}/{sort}", // URL with parameters
				new { controller = "Api", action="Search", sort = UrlParameter.Optional} // Parameter defaults
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