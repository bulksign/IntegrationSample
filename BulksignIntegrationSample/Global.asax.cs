using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using SettingsContext = BulksignIntegrationSample.Code.SettingsContext;

namespace BulksignIntegrationSample
{
	public class MvcApplication : System.Web.HttpApplication
	{
		protected void Application_Start()
		{
			AreaRegistration.RegisterAllAreas();
			GlobalConfiguration.Configure(WebApiConfig.Register);
			FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
			RouteConfig.RegisterRoutes(RouteTable.Routes);
			
			SettingsContext.DatabaseConnectionString = ConfigurationManager.AppSettings["IntegrationDatabaseConnectionString"];
			SettingsContext.BulksignRootApiUrl = ConfigurationManager.AppSettings["BulksignRootApiUrl"];


			if (string.IsNullOrWhiteSpace(SettingsContext.DatabaseConnectionString))
			{
				throw new ConfigurationErrorsException("The value for 'DatabaseConnectionString' is empty");
			}


			if (string.IsNullOrWhiteSpace(SettingsContext.BulksignRootApiUrl))
			{
				throw new ConfigurationErrorsException("The value for 'BulksignRootApiUrl' is empty");
			}


		}
	}
}
