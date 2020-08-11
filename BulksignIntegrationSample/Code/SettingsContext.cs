namespace BulksignIntegrationSample.Code
{
	public static class SettingsContext
	{

		public static string DatabaseConnectionString	  
		{
			get;
			set;
		}

		public static string BulksignRootApiUrl
		{
			get;
			set;
		}


		public static string BulksignSoapUrl => BulksignRootApiUrl + "/Bulksignapi.asmx";

		public static string BulksignRestUrl => BulksignRootApiUrl + "/v1/";

	}
}