using System.Web.Http;
using NLog;

namespace BulksignIntegration.Controllers
{
	[RoutePrefix("callback")]
	public class CallbackController : System.Web.Http.ApiController

	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public CallbackController()
		{
		}
		
		[HttpGet]
		[Route("Bundle")]
		public IHttpActionResult Bundle(string status = "", string bundleId = "", string senderEmail = "")
		{
			log.Info($"Received bundle status callback for bundleId '{bundleId}', status '{status}', sender email '{senderEmail}' ");
			return Ok();
		}

		[HttpGet]
		[Route("Recipient")]
		public IHttpActionResult Recipient(string action = "", string bundleId = "", string email = "", string senderEmail = "")
		{
			log.Info($"Received recipient action callback for recipient '{email}' , action '{action}', bundleId {bundleId}, sender email '{senderEmail}' ");
			return Ok();
		}

	}
}
