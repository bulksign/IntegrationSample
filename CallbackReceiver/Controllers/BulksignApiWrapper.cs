using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Bulksign.Api;
using BulksignIntegration.Shared;
using Newtonsoft.Json;
using NLog;

namespace BulksignIntegration.CallbackReceiver
{
	[RoutePrefix("restapi/")]
	public class BulksignApiWrapper : ApiController
	{
		public const string API_AUTHENTICATION_HEADER = "X-Bulksign-Authentication";

		private static Logger log = LogManager.GetCurrentClassLogger();

		[Route("PrepareSendEnvelope")]
		[HttpPost]
		public BulksignResult<EnvelopeApiModel> PrepareSendEnvelope([FromBody] PrepareEnvelopeApiModel prepare)
		{
			AuthenticationApiModel apiToken = ExtractApiAuthenticationToken();

			BulkSignApi api = new BulkSignApi(IntegrationSettings.BulksignRestUrl);

			BulksignResult<EnvelopeApiModel> result = api.PrepareSendEnvelope(apiToken, prepare);

			return result;
		}


		[Route("DeleteEnvelope")]
		[HttpPost]
		public BulksignResult<string> DeleteEnvelope(string envelopeId)
		{
			AuthenticationApiModel apiToken = ExtractApiAuthenticationToken();

			BulkSignApi api = new BulkSignApi(IntegrationSettings.BulksignRestUrl);

			BulksignResult<string> result = api.DeleteEnvelope(apiToken, envelopeId);

			if (result.IsSuccessful)
			{
				try
				{
					new DbIntegration().UpdateEnvelopeStatus(envelopeId, Constants.ENVELOPE_DELETED);
				}
				catch (Exception ex)
				{
					log.Error(ex, $"Failed to store envelope '{result.Response}'");
				}
			}

			return result;
		}


		[Route("SendEnvelopeFromTemplate")]
		[HttpPost]
		public BulksignResult<SendEnvelopeResultApiModel> SendEnvelopeFromTemplate([FromBody] EnvelopeFromTemplateApiModel model)
		{
			AuthenticationApiModel apiToken = ExtractApiAuthenticationToken();

			BulkSignApi api = new BulkSignApi(IntegrationSettings.BulksignRestUrl);

			BulksignResult<SendEnvelopeResultApiModel> result = api.SendEnvelopeFromTemplate(apiToken, model);

			if (result.IsSuccessful)
			{
				try
				{
					new DbIntegration().AddSentEnvelope(apiToken.UserEmail, apiToken.Key, result.Response.EnvelopeId, string.Empty);
				}
				catch (Exception ex)
				{
					log.Error(ex, $"Failed to store data about envelope '{result.Response}' ");
				}
			}

			return result;
		}


		[Route("SendEnvelope")]
		[System.Web.Mvc.HttpPost]
		public BulksignResult<SendEnvelopeResultApiModel> SendEnvelope([FromBody] EnvelopeApiModel bundle)
		{
			AuthenticationApiModel apiToken = ExtractApiAuthenticationToken();

			BulkSignApi api = new BulkSignApi(IntegrationSettings.BulksignRestUrl);

			BulksignResult<SendEnvelopeResultApiModel> result = api.SendEnvelope(apiToken, bundle);

			if (result.IsSuccessful)
			{
				try
				{
					string bundleConfiguration = IntegrationSettings.StoreEnvelopeConfiguration ? JsonConvert.SerializeObject(bundle) : string.Empty;

					new DbIntegration().AddSentEnvelope(apiToken.UserEmail, apiToken.Key, result.Response.EnvelopeId, bundleConfiguration);
				}
				catch (Exception ex)
				{
					log.Error(ex, $"Failed to store envelope '{result.Response.EnvelopeId}'");
				}
			}

			return result;
		}



		private AuthenticationApiModel ExtractApiAuthenticationToken()
		{
			try
			{
				IEnumerable<string> headerValues = null;

				//extract the authorization details
				Request.Headers.TryGetValues(API_AUTHENTICATION_HEADER, out headerValues);

				if (headerValues == null)
				{
					return null;
				}

				IEnumerable<string> values = headerValues as string[] ?? headerValues.ToArray();

				if (!values.Any())
				{
					return null;
				}

				string result = values.FirstOrDefault();

				string[] splits = result.Split(';');

				if (splits.Length != 2)
				{
					return null;
				}

				return new AuthenticationApiModel
				{
					UserEmail = splits[0],
					Key = splits[1]
				};
			}
			catch (Exception ex)
			{
				log.Error(ex);
				return null;
			}
		}


	}
}