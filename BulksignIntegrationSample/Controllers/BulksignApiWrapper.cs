﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Bulksign.Api;
using BulksignIntegrationSample.Code;
using NLog;

namespace BulksignIntegration.Controllers
{
	[RoutePrefix("restapi/")]
	public class BulksignApiWrapper : ApiController
	{
		public const string API_AUTHENTICATION_HEADER = "X-Bulksign-Authentication";

		private static Logger log = LogManager.GetCurrentClassLogger();


		[Route("SendBundle")]
		[HttpPost]
		public BulksignResult<SendBundleResultApiModel> SendBundleFromTemplate(string templatePublicId)
		{
			AuthorizationApiModel apiToken = ExtractApiAuthorization();

			BulkSignApi api = new BulkSignApi(string.Empty, SettingsContext.BulksignRestUrl);

			BulksignResult<SendBundleResultApiModel> result = api.SendBundleFromTemplate(apiToken, templatePublicId);

			if (result.IsSuccessful)
			{
				try
				{
					new DbIntegration().AddSentBundle(apiToken.UserEmail, apiToken.UserToken, result.Response.BundleId);
				}
				catch (Exception ex)
				{
					log.Error(ex, $"Failed to store bundle {result.Response}");
				}
			}

			return result;
		}


		[Route("SendBundle")]
		[HttpPost]
		public BulksignResult<SendBundleResultApiModel> SendBundle([FromBody] BundleApiModel bundle)
		{
			AuthorizationApiModel apiToken = ExtractApiAuthorization();
			
			BulkSignApi api = new BulkSignApi(string.Empty, SettingsContext.BulksignRestUrl);

			BulksignResult<SendBundleResultApiModel> result = api.SendBundle(apiToken, bundle);

			if (result.IsSuccessful)
			{
				try
				{
					new DbIntegration().AddSentBundle(apiToken.UserEmail, apiToken.UserToken, result.Response.BundleId);
				}
				catch (Exception ex)
				{
					log.Error(ex, $"Failed to store bundle {result.Response.BundleId}") ;
				}
			}

			return result;
		}

		
		[Route("SendBulkBundle")]
		[HttpPost]
		public BulksignResult<SendBundleResultApiModel> SendBulkBundle([FromBody] BundleApiModel bundle)
		{
			AuthorizationApiModel apiToken = ExtractApiAuthorization();

			BulkSignApi api = new BulkSignApi(string.Empty, SettingsContext.BulksignRestUrl);

			BulksignResult<SendBundleResultApiModel> result = api.SendBulkBundle(apiToken, bundle);

			if (result.IsSuccessful)
			{
				try
				{
					new DbIntegration().AddSentBundle(apiToken.UserEmail, apiToken.UserToken, result.Response.BundleId);
				}
				catch (Exception ex)
				{
					log.Error(ex, $"Failed to store bundle {result.Response.BundleId}");
				}
			}

			return result;
		}



		
		private AuthorizationApiModel ExtractApiAuthorization()
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

				return new AuthorizationApiModel { UserEmail = splits[0], UserToken = splits[1] };
			}
			catch (Exception ex)
			{
				log.Error(ex);
				return null;
			}
		}


	}
}