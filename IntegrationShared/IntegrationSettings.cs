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

		public static int IntervalEnvelopeStatusInHours
		{
			get;
			set;
		}

		public static string DatabaseConnectionString
		{
			get;
			set;
		}

		public static string CompletedEnvelopePath
		{
			get;
			set;
		}

		public static bool StoreEnvelopeConfiguration
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
			IntervalEnvelopeStatusInHours = Convert.ToInt32(ConfigurationManager.AppSettings["IntervalBundleStatusInMinutes"]);
			DatabaseConnectionString = ConfigurationManager.AppSettings["DatabaseConnectionString"];
			CompletedEnvelopePath = ConfigurationManager.AppSettings["CompletedBundlePath"];
			StoreEnvelopeConfiguration = Convert.ToBoolean(ConfigurationManager.AppSettings["StoreEnvelopeConfiguration"]);
			RootApiUrl = ConfigurationManager.AppSettings["BulksignRootApiUrl"];
		}
	}
}