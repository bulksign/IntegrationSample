using System;
using System.Configuration;

namespace BulksignIntegration.Shared
{
	public static class IntegrationSettings
	{
		public static int IntervalCompletedDocumentsInMinutes
		{
			get;
			set;
		}

		public static int IntervalBundleStatusInHours
		{
			get;
			set;
		}

		public static string DatabaseConnectionString
		{
			get;
			set;
		}

		public static string CompletedBundlePath
		{
			get;
			set;
		}

		public static bool StoreBundleConfiguration
		{
			get;
			set;
		}

		public static string RootApiUrl
		{
			get;
			set;
		}

		public static string BulksignRestUrl => RootApiUrl + "/restapi/";




		public static void ReadSettings()
		{
			IntervalCompletedDocumentsInMinutes = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalCompletedDocumentsInMinutes"]);
			IntervalBundleStatusInHours = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalBundleStatusInMinutes"]);
			DatabaseConnectionString = ConfigurationManager.AppSettings["DatabaseConnectionString"];
			CompletedBundlePath = ConfigurationManager.AppSettings["CompletedBundlePath"];
			StoreBundleConfiguration = Convert.ToBoolean(ConfigurationManager.AppSettings["StoreBundleConfiguration"]);
			RootApiUrl = ConfigurationManager.AppSettings["BulksignRootApiUrl"];
		}
	}
}