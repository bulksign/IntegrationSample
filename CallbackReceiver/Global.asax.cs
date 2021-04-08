using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using BulksignIntegration.Shared;
using NLog;

namespace BulksignIntegration.CallbackReceiver
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);

			IntegrationSettings.ReadSettings();

			LogManager.GetCurrentClassLogger().Log(LogLevel.Info, "Started callback receiver");
		}
	}
}
