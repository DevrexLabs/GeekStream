using System.Configuration;
using System.Linq.Expressions;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using GeekStream.Core.Domain;
using OrigoDB.Core;
using OrigoDB.Core.Proxy;

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
					"api/v1/{sort}", // URL with parameters
					new { controller = "Api", action = "Items"} // Parameter defaults
					);

			routes.MapRoute("ApiSearchRoute", // Route name
				"api/v1/search/{query}/{sort}", // URL with parameters
				new { controller = "Api", action="Search", sort = UrlParameter.Optional} // Parameter defaults
				);

		}

		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			RegisterGlobalFilters(GlobalFilters.Filters);
			RegisterRoutes(RouteTable.Routes);
		}



	    private static IEngine<GeekStreamModel> _engine;

	    public static IEngine<GeekStreamModel> DbClient
	    {
	        get
	        {
                if (_engine == null)
                {
                    var connectionStringElement = ConfigurationManager.ConnectionStrings["geekstream"];
                    if (connectionStringElement != null)
                        _engine = Engine.For<GeekStreamModel>(connectionStringElement.ConnectionString);
                    else _engine = Engine.For<GeekStreamModel>();
                }
	            return _engine;

	        }
	    }
		public static GeekStreamModel DbProxy
		{
			get
			{
			    return DbClient.GetProxy();
			}
		}
    }
}