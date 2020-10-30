using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using BulksignIntegration.Shared;

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

			//IntegrationSettings.DatabaseConnectionString = ConfigurationManager.AppSettings["IntegrationDatabaseConnectionString"];
			//IntegrationSettings.RootApiUrl = ConfigurationManager.AppSettings["BulksignRootApiUrl"];


			//if (string.IsNullOrWhiteSpace(IntegrationSettings.DatabaseConnectionString))
			//{
			//	throw new ConfigurationErrorsException("The value for 'DatabaseConnectionString' is empty");
			//}


			//if (string.IsNullOrWhiteSpace(IntegrationSettings.RootApiUrl))
			//{
			//	throw new ConfigurationErrorsException("The value for 'BulksignRootApiUrl' is empty");
			//}


		}
	}
}
