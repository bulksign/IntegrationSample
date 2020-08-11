using System;
using System.Data.SqlClient;
using NLog;

namespace BulksignIntegrationSample.Code
{
	public class DbIntegration
	{

		private static Logger log = LogManager.GetCurrentClassLogger();

		public void AddSentBundle(string email, string token, string bundleId)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = SettingsContext.DatabaseConnectionString;
					connection.Open();

					SqlCommand command = new SqlCommand();

					command.Parameters.AddWithValue("bundleId", bundleId);
					command.Parameters.AddWithValue("status", Constants.NUMERIC_BUNDLE_STATUS_IN_PROGRESS);
					command.Parameters.AddWithValue("sentDate", DateTime.UtcNow);
					command.Parameters.AddWithValue("senderEmail", email);
					command.Parameters.AddWithValue("apiKey", token);
					command.Parameters.AddWithValue("processed", 0);

					command.Connection = connection;

					command.CommandText = "INSERT INTO SentBundles(BundleId,SentDate,Status,SenderEmail,ApiKey, IsProcessed) VALUES (@bundleId, @sentDate, @status,@senderEmail,@apiKey, @processed)";

					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
		}

		public void UpdateBundleStatus(string bundleId, string statusAsString)
		{

			try
			{
				int newStatus = ConvertBundleStatus(statusAsString);

				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = SettingsContext.DatabaseConnectionString;
					connection.Open();


					SqlCommand command = new SqlCommand();

					command.Parameters.AddWithValue("bundleId", bundleId);
					command.Connection = connection;

					command.CommandText = "SELECT Status FROM SentBundles WHERE BundleId = @bundleId";

					object result = command.ExecuteScalar();


					if (result == null || result == DBNull.Value)
					{
						log.Error($"Bundle '{bundleId}' was not found in database");
						return;
					}


					if (result.ToString() != Constants.NUMERIC_BUNDLE_STATUS_IN_PROGRESS.ToString())
					{
						log.Error($"Bundle '{bundleId}' received invalid cllback status. Current DB status is '{result}', received status is {newStatus} ");
						return;
					}

					command.Parameters.Clear();

					command.Parameters.AddWithValue("newStatus", newStatus);
					command.Parameters.AddWithValue("bundleId", bundleId);

					command.CommandText = "UPDATE SentBundles SET Status=@newStatus WHERE BundleId=@bundleId";

					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex);
			}
		}


		private int ConvertBundleStatus(string status)
		{
			switch (status)
			{
				case Constants.ENVELOPE_CANCELED:
					return Constants.NUMERIC_BUNDLE_STATUS_CANCELED;

				case Constants.ENVELOPE_EXPIRED:
					return Constants.NUMERIC_BUNDLE_STATUS_EXPIRED;

				case Constants.ENVELOPE_COMPLETED:
					return Constants.NUMERIC_BUNDLE_STATUS_COMPLETED;

				default:
					throw  new ArgumentException($"Bundle Status '{status}' is invalid ");
			}
		}
	}





}