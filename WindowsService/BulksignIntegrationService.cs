using System;
using System.ServiceProcess;
using System.Timers;
using NLog.Fluent;

namespace BulksignIntegration.WindowsService
{
	public partial class BulksignIntegrationService : ServiceBase
	{
		public BulksignIntegrationService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
			new ServiceStarter().Run();
		}

		protected override void OnStop()
		{
			Log.Info($"BulksignIntegration service stopped");
		}

	}
}