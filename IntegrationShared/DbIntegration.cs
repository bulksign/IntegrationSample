using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using NLog;

namespace BulksignIntegration.Shared
{
	public class DbIntegration
	{
		private static Logger log = LogManager.GetCurrentClassLogger();

		public SentEnvelopeModel[] GetInProgressEnvelopes()
		{
			try
			{
				List<SentEnvelopeModel> result = new List<SentEnvelopeModel>();

				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
					connection.Open();

					SqlCommand command = new SqlCommand();
					command.Connection = connection;

					command.CommandText = "SELECT EnvelopeId, SenderEmail, ApiKey FROM SentEnvelopes WHERE Status=" + Constants.NUMERIC_ENVELOPE_STATUS_IN_PROGRESS;

					SqlDataReader reader = command.ExecuteReader();

					while (reader.Read())
					{
						SentEnvelopeModel sm = new SentEnvelopeModel();
						sm.Id = reader[0].ToString();
						sm.SenderEmail = reader[1].ToString();
						sm.ApiToken = reader[2].ToString();

						result.Add(sm);
					}
				}

				return result.ToArray();
			}
			catch (Exception ex)
			{
				log.Error(ex, $" {nameof(GetInProgressEnvelopes)}");
				return new SentEnvelopeModel[0]; 
			}
		}

		public SentEnvelopeModel[] GetEnvelopesToDownload()
		{
			try
			{
				List<SentEnvelopeModel> result = new List<SentEnvelopeModel>();

				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
					connection.Open();

					SqlCommand command = new SqlCommand();
					command.Connection = connection;

					command.CommandText = "SELECT EnvelopeId, SenderEmail, ApiKey FROM SentEnvelopes WHERE Status=" + Constants.NUMERIC_ENVELOPE_STATUS_COMPLETED + " AND CompletedFilePath=" + DBNull.Value;

					SqlDataReader reader = command.ExecuteReader();

					while (reader.Read())
					{
						SentEnvelopeModel sm = new SentEnvelopeModel();
						sm.Id = reader[0].ToString();
						sm.SenderEmail = reader[1].ToString();
						sm.ApiToken = reader[2].ToString();

						result.Add(sm);
					}
				}

				return result.ToArray();
			}
			catch (Exception ex)
			{
				log.Error(ex, $" {nameof(GetEnvelopesToDownload)}");
				return new SentEnvelopeModel[0];
			}
		}

		public void MarkAsDeleted(string envelopeId)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
					connection.Open();

					SqlCommand command = new SqlCommand();

					command.Parameters.AddWithValue("envelopeId", envelopeId);
					command.Parameters.AddWithValue("status", Constants.NUMERIC_ENVELOPE_STATUS_DELETED);
					command.Connection = connection;

					command.CommandText = "UPDATE SentEnvelopes SET Status=@status WHERE EnvelopeId=@envelopeId";

					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex, $"Failed to insert envelope {envelopeId}");
			}
		}

		public void AddSentEnvelope(string email, string token, string envelopeId, string envelopeConfiguration)
		{
			try
			{
				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
					connection.Open();

					SqlCommand command = new SqlCommand();

					command.Parameters.AddWithValue("envelopeId", envelopeId);
					command.Parameters.AddWithValue("status", Constants.NUMERIC_ENVELOPE_STATUS_IN_PROGRESS);
					command.Parameters.AddWithValue("sentDate", DateTime.UtcNow);
					command.Parameters.AddWithValue("senderEmail", email);
					command.Parameters.AddWithValue("apiKey", token);
					command.Parameters.AddWithValue("processed", 0);
					command.Parameters.AddWithValue("sentEnvelopeConfiguration", envelopeConfiguration);


					command.Connection = connection;

					command.CommandText = "INSERT INTO SentEnvelopes(envelopeId,SentDate,Status,SenderEmail,ApiKey, IsProcessed,SentEnvelopeConfiguration) VALUES (@envelopeId, @sentDate, @status,@senderEmail,@apiKey, @processed, @sentEnvelopeConfiguration)";

					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex, $"Failed to insert envelope '{envelopeId}'");
			}
		}

		public void UpdateEnvelopeStatus(string envelopeId, string statusAsString)
		{

			try
			{
				int newStatus = ConvertEnvelopeStatus(statusAsString);

				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
					connection.Open();


					SqlCommand command = new SqlCommand();

					command.Parameters.AddWithValue("envelopeId", envelopeId);
					command.Connection = connection;

					command.CommandText = "SELECT Status FROM SentEnvelopes WHERE EnvelopeId = @envelopeId";

					object result = command.ExecuteScalar();


					if (result == null || result == DBNull.Value)
					{
						log.Error($"Envelope '{envelopeId}' was not found in database");
						return;
					}


					if (result.ToString() != Constants.NUMERIC_ENVELOPE_STATUS_IN_PROGRESS.ToString())
					{
						log.Error($"Envelope '{envelopeId}' received invalid callback status. Current DB status is '{result}', received status is {newStatus} ");
						return;
					}

					command.Parameters.Clear();

					command.Parameters.AddWithValue("newStatus", newStatus);
					command.Parameters.AddWithValue("enveloped", envelopeId);

					command.CommandText = "UPDATE SentEnvelopes SET Status=@newStatus WHERE EnvelopeId=@envelopeId";

					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex, $"Failed to update status for envelope '{envelopeId}' ");
			}
		}

		public void UpdateEnvelopeCompletedDocumentPath(string envelopeId, string fullPath)
		{

			try
			{
				using (SqlConnection connection = new SqlConnection())
				{
					connection.ConnectionString = IntegrationSettings.DatabaseConnectionString;
					connection.Open();

					SqlCommand command = new SqlCommand();
					command.Connection = connection;

					command.Parameters.AddWithValue("envelopeId", envelopeId);
					command.Parameters.AddWithValue("completedPath", fullPath);

					command.CommandText = "UPDATE SentEnvelopes SET CompletedFilePath=@completedPath WHERE EnvelopeId=@envelopeId";

					command.ExecuteNonQuery();
				}
			}
			catch (Exception ex)
			{
				log.Error(ex, $"Failed {nameof(UpdateEnvelopeCompletedDocumentPath)} for '{envelopeId}' ");
			}
		}

		private int ConvertEnvelopeStatus(string status)
		{
			switch (status)
			{
				case Constants.ENVELOPE_CANCELED:
					return Constants.NUMERIC_ENVELOPE_STATUS_CANCELED;

				case Constants.ENVELOPE_EXPIRED:
					return Constants.NUMERIC_ENVELOPE_STATUS_EXPIRED;

				case Constants.ENVELOPE_COMPLETED:
					return Constants.NUMERIC_ENVELOPE_STATUS_COMPLETED;

				case Constants.ENVELOPE_DELETED:
					return Constants.NUMERIC_ENVELOPE_STATUS_DELETED;

				default:
					throw  new ArgumentException($"Envelope status '{status}' is invalid ");
			}
		}
	}





}