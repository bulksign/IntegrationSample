using System;
using System.Threading;
using System.Timers;
using BulksignIntegration.Shared;
using NLog;
using Timer = System.Timers.Timer;

namespace BulksignIntegration.WindowsService
{
	public class ServiceStarter
	{
		private static ILogger log = LogManager.GetCurrentClassLogger();

		private const int ONE_MINUTE_IN_MILISECONDS = 60000;

		public static bool IS_RUNNING = false;

		private static Timer timerDownloadCompletedDocuments = new Timer { AutoReset = true, Enabled = true };

		private static Timer timerStatusUpdate = new Timer { AutoReset = true, Enabled = true };


		public void Run()
		{
			try
			{
				log.Info($"BulksignIntegration service started");

				timerDownloadCompletedDocuments.Enabled = true;
				timerDownloadCompletedDocuments.AutoReset = true;
				timerDownloadCompletedDocuments.Interval = ONE_MINUTE_IN_MILISECONDS * IntegrationSettings.IntervalCompletedDocumentsInMinutes;
				timerDownloadCompletedDocuments.Elapsed += TimerDownloadCompletedDocumentsElapsed;

				timerStatusUpdate.Enabled = true;
				timerStatusUpdate.AutoReset = true;
				timerStatusUpdate.Interval = ONE_MINUTE_IN_MILISECONDS * 60 * IntegrationSettings.IntervalBundleStatusInHours * 24;
				timerStatusUpdate.Elapsed += TimerStatusUpdateElapsed;


				//run them now
				TimerDownloadCompletedDocumentsElapsed(null, null);
			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
		}

		private void TimerStatusUpdateElapsed(object sender, ElapsedEventArgs e)
		{
			try
			{
				DbIntegration db = new DbIntegration();

				SentBundleModel[] inProgress = db.GetInProgressBundles();

				BulksignIntegration bi = new BulksignIntegration();

				foreach (SentBundleModel m in inProgress)
				{
					try
					{
						bi.CheckBundleStatus(m);

						Thread.Sleep(1000);
					}
					catch (Exception ex)
					{
						log.Error(ex);
					}
				}
			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
		}

		private void TimerDownloadCompletedDocumentsElapsed(object sender, ElapsedEventArgs e)
		{
			if (IS_RUNNING)
			{
				return;
			}

			try
			{
				IS_RUNNING = true;

				DbIntegration db = new DbIntegration();

				SentBundleModel[] bundlesToProcess = db.GetBundlesToDownload();

				BulksignIntegration bi = new BulksignIntegration();

				foreach (SentBundleModel model in bundlesToProcess)
				{
					try
					{
						bi.DownloadCompleted(model.Id, model.ApiKey, model.SenderEmail);

						Thread.Sleep(1000);
					}
					catch (Exception ex)
					{
						log.Error(ex);
					}
				}
			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
			finally
			{
				IS_RUNNING = false;
			}
		}
	}
}