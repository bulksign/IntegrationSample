using System.Web.Http;
using BulksignIntegration.Shared;
using NLog;

namespace BulksignIntegration.CallbackReceiver
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
			log.Info($"Received bundle status callback :  bundleId '{bundleId}', status '{status}', sender email '{senderEmail}' ");

			//check the bundleId
			if (string.IsNullOrWhiteSpace(bundleId))
			{
				log.Error("Invalid bundle status callback received, bundleId is empty");
				return Ok();
			}
			
			//check the status
			if (status.ToLower() == Constants.ENVELOPE_COMPLETED || status.ToLower() == Constants.ENVELOPE_CANCELED || status.ToLower() == Constants.ENVELOPE_EXPIRED)
			{
				new DbIntegration().UpdateBundleStatus(bundleId, status);
			}
			else
			{
				log.Error($"Invalid callback received, unknown status '{status}' and bundleId {bundleId}");
				return Ok();
			}


			//TODO : add your own code here to deal with the status change

			return Ok();
		}

		[HttpGet]
		[Route("Recipient")]
		public IHttpActionResult Recipient(string action = "", string bundleId = "", string email = "", string senderEmail = "")
		{
			log.Info($"Received recipient action callback : recipient '{email}' , action '{action}', bundleId {bundleId}, sender email '{senderEmail}' ");


			if (string.IsNullOrWhiteSpace(bundleId))
			{
				log.Error("Invalid recipient action callback received, bundleId is empty");
				return Ok();
			}

			//add your own code here to process recipient actions

			switch (action.ToLower())
			{

				//for bundles with multiple signers, you receive this callback when is the turn for a new signer
				case Constants.RECIPIENT_ACTION_NEXT_SIGNER:
					break;

				//it means a signer opened the sign step to sign (this is only triggered once per recipient)
				case Constants.RECIPIENT_ACTION_OPENED:
					break;

				//signer rejected to sign the document
				case Constants.RECIPIENT_ACTION_SIGN_STEP_REJECTED:
					break;

				//signer successfully finished the sign step 
				case Constants.RECIPIENT_ACTION_SIGN_STEP_FINISHED:
					break;

				//the received action is invalid, log it and ignore the callback
				default:
					log.Error($"Invalid callback action received '{action}', bundleId '{bundleId}' ");
					break;
			}

			return Ok();
		}

	}
}
