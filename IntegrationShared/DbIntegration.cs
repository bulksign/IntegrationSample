using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NLog;

namespace BulksignIntegration.Shared
{
	public class DbIntegration
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public SentBundleModel[] GetInProgressBundles()
		{
			try
			{
				List<SentBundleModel> result = new List<SentBundleModel>();

				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
					connection.Open();

					SqlCommand command = new SqlCommand();
					command.Connection = connection;

					command.CommandText = "SELECT BundleId, SenderEmail, ApiKey FROM SentBundles WHERE Status=" + Constants.NUMERIC_BUNDLE_STATUS_IN_PROGRESS;

					SqlDataReader reader = command.ExecuteReader();

					while (reader.Read())
					{
						SentBundleModel sm = new SentBundleModel();
						sm.Id = reader[0].ToString();
						sm.SenderEmail = reader[1].ToString();
						sm.ApiKey = reader[2].ToString();

						result.Add(sm);
					}
				}

				return result.ToArray();
			}
			catch (Exception ex)
			{
				log.Error(ex, $" {nameof(GetInProgressBundles)}");
				return new SentBundleModel[0]; 
			}
		}

		public SentBundleModel[] GetBundlesToDownload()
		{
			try
			{
				List<SentBundleModel> result = new List<SentBundleModel>();

				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
					connection.Open();

					SqlCommand command = new SqlCommand();
					command.Connection = connection;

					command.CommandText = "SELECT BundleId, SenderEmail, ApiKey FROM SentBundles WHERE Status=" + Constants.NUMERIC_BUNDLE_STATUS_COMPLETED + " AND CompletedFilePath=" + DBNull.Value;

					SqlDataReader reader = command.ExecuteReader();

					while (reader.Read())
					{
						SentBundleModel sm = new SentBundleModel();
						sm.Id = reader[0].ToString();
						sm.SenderEmail = reader[1].ToString();
						sm.ApiKey = reader[2].ToString();

						result.Add(sm);
					}
				}

				return result.ToArray();
			}
			catch (Exception ex)
			{
				log.Error(ex, $" {nameof(GetBundlesToDownload)}");
				return new SentBundleModel[0];
			}
		}

		public void MarkAsDeleted(string bundleId)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
					connection.Open();

					SqlCommand command = new SqlCommand();

					command.Parameters.AddWithValue("bundleId", bundleId);
					command.Parameters.AddWithValue("status", Constants.NUMERIC_BUNDLE_STATUS_DELETED);
					command.Connection = connection;

					command.CommandText = "UPDATE SentBundles SET Status=@status WHERE BundleId=@bundleId";

					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex, $"Failed to insert bundle {bundleId}");
			}
		}

		public void AddSentBundle(string email, string token, string bundleId, string bundleConfiguration)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
					connection.Open();

					SqlCommand command = new SqlCommand();

					command.Parameters.AddWithValue("bundleId", bundleId);
					command.Parameters.AddWithValue("status", Constants.NUMERIC_BUNDLE_STATUS_IN_PROGRESS);
					command.Parameters.AddWithValue("sentDate", DateTime.UtcNow);
					command.Parameters.AddWithValue("senderEmail", email);
					command.Parameters.AddWithValue("apiKey", token);
					command.Parameters.AddWithValue("processed", 0);
					command.Parameters.AddWithValue("sentBundleConfiguration", bundleConfiguration);


					command.Connection = connection;

					command.CommandText = "INSERT INTO SentBundles(BundleId,SentDate,Status,SenderEmail,ApiKey, IsProcessed,SentBundleConfiguration) VALUES (@bundleId, @sentDate, @status,@senderEmail,@apiKey, @processed, @sentBundleConfiguration)";

					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex, $"Failed to insert bundle {bundleId}");
			}
		}

		public void UpdateBundleStatus(string bundleId, string statusAsString)
		{

			try
			{
				int newStatus = ConvertBundleStatus(statusAsString);

				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
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
						log.Error($"Bundle '{bundleId}' received invalid callback status. Current DB status is '{result}', received status is {newStatus} ");
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
				log.Error(ex, $"Failed to update status for bundle {bundleId}");
			}
		}

		public void UpdateBundleCompletedDocumentPath(string bundleId, string fullPath)
		{

			try
			{
				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
					connection.Open();

					SqlCommand command = new SqlCommand();
					command.Connection = connection;

					command.Parameters.AddWithValue("bundleId", bundleId);
					command.Parameters.AddWithValue("completedPath", fullPath);

					command.CommandText = "UPDATE SentBundles SET CompletedFilePath=@completedPath WHERE BundleId=@bundleId";

					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex, $"Failed {nameof(UpdateBundleCompletedDocumentPath)} for '{bundleId}' ");
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

				case Constants.ENVELOPE_DELETED:
					return Constants.NUMERIC_BUNDLE_STATUS_DELETED;

				default:
					throw  new ArgumentException($"Bundle Status '{status}' is invalid ");
			}
		}
	}





}