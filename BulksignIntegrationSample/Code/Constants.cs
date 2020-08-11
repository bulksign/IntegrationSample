namespace BulksignIntegrationSample.Code
{
	public class Constants
	{
		public const string ENVELOPE_COMPLETED = "completed";
		public const string ENVELOPE_CANCELED = "canceled";
		public const string ENVELOPE_EXPIRED = "expired";


		public const int NUMERIC_BUNDLE_STATUS_IN_PROGRESS = 3;
		
		public const int NUMERIC_BUNDLE_STATUS_COMPLETED = 4;
		public const int NUMERIC_BUNDLE_STATUS_CANCELED = 5;
		public const int NUMERIC_BUNDLE_STATUS_EXPIRED = 6;



		public const string RECIPIENT_ACTION_OPENED = "opened";
		public const string RECIPIENT_ACTION_SIGN_STEP_FINISHED = "signstepfinished";
		public const string RECIPIENT_ACTION_SIGN_STEP_REJECTED = "signsteprejected";
		public const string RECIPIENT_ACTION_NEXT_SIGNER = "nextsigner";
	}
}