using System;
using System.Data;
using Microsoft.Data.SqlClient;

namespace IMS_API.Utilities
{
    public static class AppLogic
    {
        private static readonly string _connectionString;

        // Static constructor to initialize the connection string
        static AppLogic()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _connectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(_connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured in appsettings.json.");
            }
        }
        // Method to log exceptions into the database
        public static void LogException(string functionName, string errorMessage, string stackTrace)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                using (var command = new SqlCommand("spLogException", connection))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Add(new SqlParameter("@FunctionName", SqlDbType.NVarChar) { Value = functionName });
                    command.Parameters.Add(new SqlParameter("@ErrorMessage", SqlDbType.NVarChar) { Value = errorMessage });
                    command.Parameters.Add(new SqlParameter("@StackTrace", SqlDbType.NVarChar) { Value = stackTrace });
                    command.Parameters.Add(new SqlParameter("@LogDate", SqlDbType.DateTime) { Value = DateTime.Now });

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
