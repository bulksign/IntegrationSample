using System;
using System.IO;
using Bulksign.Api;
using Bulksign.DomainLogic.Api;
using BulksignIntegration.Shared;
using NLog;

namespace BulksignIntegration.WindowsService
{
	public class BulksignIntegration
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public void CheckEnvelopeStatus(SentEnvelopeModel envelope)
		{
			try
			{
				log.Info($"Checking status for envelope '{envelope.Id}' ");

				BulksignApiClient api = new BulksignApiClient(IntegrationSettings.BulksignRestUrl);

				AuthenticationApiModel token = new AuthenticationApiModel()
				{
					UserEmail = envelope.SenderEmail,
					Key = envelope.ApiToken
				};

				BulksignResult<EnvelopeStatusTypeApi> envelopeStatus = api.GetEnvelopeStatus(token, envelope.Id);


				if (envelopeStatus.IsSuccessful == false)
				{
					//it seems the envelope doesn't exist anymore, must have been deleted
					if (envelopeStatus.ErrorCode == ApiErrorCode.API_ERROR_CODE_ENVELOPE_WITH_ID_NOT_FOUND)
					{
						log.Error($"{nameof(DownloadCompletedDocuments)} , GetEnvelopeStatus returned {ApiErrorCode.API_ERROR_MESSAGE_ENVELOPE_WITH_ID_NOT_FOUND} , envelope doesn't exists");

						new DbIntegration().UpdateEnvelopeStatus(envelope.Id, Constants.ENVELOPE_DELETED);
					}
					else
					{
						log.Error($" {nameof(DownloadCompletedDocuments)} failed for '{envelope.Id}', calling GetEnvelopeStatus failed with code : '{envelopeStatus.ErrorCode}', message : '{envelopeStatus.ErrorMessage}' ");
					}

					return;
				}

				//was the status changed 
				if ((int) envelopeStatus.Response != Constants.NUMERIC_ENVELOPE_STATUS_IN_PROGRESS)
				{
					log.Info($"Updating status for envelope '{envelope.Id}', new status is '{envelopeStatus.Response.ToString()}' ");
					new DbIntegration().UpdateEnvelopeStatus(envelope.Id, envelopeStatus.Response.ToString());
				}
				else
				{
					log.Info($"Status for envelope '{envelope.Id}' is not changed");
				}
			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
		}

		public void DownloadCompletedDocuments(string envelopeId, string apiKey, string email)
		{
			log.Info($"Preparing to download documents for completed envelope '{envelopeId}' ");

			BulksignApiClient api = new BulksignApiClient(IntegrationSettings.BulksignRestUrl);

			AuthenticationApiModel token = new AuthenticationApiModel()
			{
				UserEmail = email,
				Key = apiKey
			};

			BulksignResult<EnvelopeStatusTypeApi> envelopeStatus = api.GetEnvelopeStatus(token, envelopeId);

			if (envelopeStatus.IsSuccessful == false)
			{
				//it seems the envelope doesn't exist
				if (envelopeStatus.ErrorCode == ApiErrorCode.API_ERROR_CODE_ENVELOPE_WITH_ID_NOT_FOUND)
				{
					log.Error($"{nameof(DownloadCompletedDocuments)} , for envelopeId '{envelopeId}', GetEnvelopeStatus returned '{ApiErrorCode.API_ERROR_MESSAGE_ENVELOPE_WITH_ID_NOT_FOUND}' , envelope doesn't exists anymore");
					
					new DbIntegration().UpdateEnvelopeStatus(envelopeId, Constants.ENVELOPE_DELETED);
				}
				else
				{
					log.Error($" {nameof(DownloadCompletedDocuments)} failed for '{envelopeId}', calling GetEnvelopeStatus failed with code : '{envelopeStatus.ErrorCode}', message : '{envelopeStatus.ErrorMessage}' ");
				}

				return;
			}

			if (envelopeStatus.Response != EnvelopeStatusTypeApi.Completed)
			{
				log.Error($" Invalid envelope status received in {nameof(DownloadCompletedDocuments)} for envelopeId '{envelopeId}', status {envelopeStatus.ToString()} ");

				//just update the status
				new DbIntegration().UpdateEnvelopeStatus(envelopeId, envelopeStatus.ToString().ToLower());
				return;
			}

			BulksignResult<byte[]> result = api.DownloadEnvelopeCompletedDocuments(token, envelopeId);

			if (result.IsSuccessful == false)
			{
				log.Error($" {nameof(DownloadCompletedDocuments)} failed, code : {result.ErrorCode}, message : {result.ErrorMessage} ");
				return;
			}

			string fullPath = IntegrationSettings.CompletedEnvelopePath + @"\" + envelopeId + ".zip";

			File.WriteAllBytes(fullPath, result.Response);

			new DbIntegration().UpdateEnvelopeCompletedDocumentPath(envelopeId, fullPath);
		}

	}
}