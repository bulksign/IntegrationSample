using System;
using System.IO;
using Bulksign.Api;
using Bulksign.DomainLogic.Api;
using BulksignIntegration.Shared;
using NLog;
using NLog.Fluent;

namespace BulksignIntegration.WindowsService
{
	public class BulksignIntegration
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public void CheckBundleStatus(SentBundleModel bundle)
		{
			try
			{
				log.Info($"Checking status for bundle '{bundle.Id}' ");

				BulkSignApi api = new BulkSignApi(IntegrationSettings.BulksignRestUrl);

				AuthorizationApiModel token = new AuthorizationApiModel()
				{
					UserEmail = bundle.SenderEmail,
					UserToken = bundle.ApiKey
				};

				BulksignResult<BundleStatusApi> bundleStatus = api.GetBundleStatus(token, bundle.Id);


				if (bundleStatus.IsSuccessful == false)
				{
					//it seems the bundle doesn't exist anymore, must have been deleted
					if (bundleStatus.ErrorCode == ApiErrorCode.API_ERROR_CODE_SPECIFIED_ID_NOT_BUNDLE)
					{
						log.Error($"{nameof(DownloadCompleted)} , GetBundleStatus returned {ApiErrorCode.API_ERROR_CODE_SPECIFIED_ID_NOT_BUNDLE} , bundle doesn't exists");

						new DbIntegration().UpdateBundleStatus(bundle.Id, Constants.ENVELOPE_DELETED);
					}
					else
					{
						log.Error($" {nameof(DownloadCompleted)} failed for '{bundle.Id}', calling GetBundleStatus failed with code : '{bundleStatus.ErrorCode}', message : '{bundleStatus.ErrorMessage}' ");
					}

					return;
				}

				//was the status changed 
				if ((int) bundleStatus.Response != Constants.NUMERIC_BUNDLE_STATUS_IN_PROGRESS)
				{
					log.Info($"Updating status for bundle {bundle.Id},new status is {bundleStatus.Response.ToString()} ");
					new DbIntegration().UpdateBundleStatus(bundle.Id, bundleStatus.Response.ToString());
				}
				else
				{
					log.Info($"Status for bundle {bundle.Id} in not changed");
				}
			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
		}

		public void DownloadCompleted(string bundleId, string apiKey, string email)
		{
			log.Info($"Preparing to download documents for completed bundle {bundleId} ");

			BulkSignApi api = new BulkSignApi(IntegrationSettings.BulksignRestUrl);

			AuthorizationApiModel token = new AuthorizationApiModel()
			{
				UserEmail = email,
				UserToken = apiKey
			};

			BulksignResult<BundleStatusApi> bundleStatus = api.GetBundleStatus(token, bundleId);

			if (bundleStatus.IsSuccessful == false)
			{
				//it seems the bundle doesn't exist
				if (bundleStatus.ErrorCode == ApiErrorCode.API_ERROR_CODE_SPECIFIED_ID_NOT_BUNDLE)
				{
					log.Error($"{nameof(DownloadCompleted)} , GetBundleStatus returned {ApiErrorCode.API_ERROR_CODE_SPECIFIED_ID_NOT_BUNDLE} , bundle doesn't exists");
					
					new DbIntegration().UpdateBundleStatus(bundleId, Constants.ENVELOPE_DELETED);
				}
				else
				{
					log.Error($" {nameof(DownloadCompleted)} failed for '{bundleId}', calling GetBundleStatus failed with code : '{bundleStatus.ErrorCode}', message : '{bundleStatus.ErrorMessage}' ");
				}

				return;
			}

			if (bundleStatus.Response != BundleStatusApi.Completed)
			{
				log.Error($" Invalid bundle status received in {nameof(DownloadCompleted)} for bundleId '{bundleId}', status {bundleStatus.ToString()} ");

				//just update the status
				new DbIntegration().UpdateBundleStatus(bundleId, bundleStatus.ToString().ToLower());
				return;
			}

			BulksignResult<byte[]> result = api.GetCompletedDocuments(token, bundleId);

			if (result.IsSuccessful == false)
			{
				log.Error($" {nameof(DownloadCompleted)} failed, code : {result.ErrorCode}, message : {result.ErrorMessage} ");
				return;
			}

			string fullPath = IntegrationSettings.CompletedBundlePath + @"\" + bundleId + ".zip";

			File.WriteAllBytes(fullPath, result.Response);

			new DbIntegration().UpdateBundleCompletedDocumentPath(bundleId, fullPath);
		}

	}
}