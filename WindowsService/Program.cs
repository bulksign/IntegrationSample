using System;
using System.Threading;
using BulksignIntegration.Shared;
using NLog;

namespace BulksignIntegration.WindowsService
{
	public static class Program
	{

		private static ILogger log = LogManager.GetCurrentClassLogger();

#if DEBUG
		[STAThread]
#endif
		public static void Main()
		{
			try
			{

				IntegrationSettings.ReadSettings();

#if DEBUG
				RunInCommandLineMode();
#else
				ServiceBase.Run(new BulksignService());
#endif

			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
		}

		private static void RunInCommandLineMode()
		{
			new ServiceStarter().Run();
			while (true)
			{
				Thread.Sleep(1);
			}
		}
	}
}