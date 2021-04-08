using System.Web;
using System.Web.Mvc;

namespace BulksignIntegration.CallbackReceiver
{
	public class FilterConfig
	{
		public static void RegisterGlobalFilters(GlobalFilterCollection filters)
		{
			filters.Add(new HandleErrorAttribute());
		}
	}
}
