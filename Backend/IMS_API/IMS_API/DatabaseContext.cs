using System;
using System.Data;
using IMS_API.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace IMS_API
{
    public class DatabaseContext
    {
        public string ConnectionString { get; }

        public DatabaseContext(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration), "Configuration cannot be null.");

            ConnectionString = configuration.GetConnectionString("DefaultConnection");

            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        // Add GetConnectionString method to return the connection string
        public string GetConnectionString()
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
                throw new InvalidOperationException("Database connection string is null or empty.");

            return ConnectionString;
        }
    }

    public class DatabaseConnectionProvider : IDatabaseConnectionProvider
    {
        private readonly DatabaseContext _context;

        public DatabaseConnectionProvider(DatabaseContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public IDbConnection CreateConnection()
        {
            //var connectionString = _context.GetConnectionString(); // Use the new GetConnectionString() method
            //var connection = new SqlConnection(connectionString);
            //connection.ConnectionString += ";Pooling=true;Min Pool Size=5;Max Pool Size=4096;";
            //return connection;

            return new SqlConnection(_context.GetConnectionString());
        }
    }
}
