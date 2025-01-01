using System.Data;
using Microsoft.Data.SqlClient; 
using Microsoft.Extensions.Configuration;

namespace IMS_API
{
    public class DatabaseContext
    {
        private readonly string _connectionString;

        public DatabaseContext(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString); // Use Microsoft.Data.SqlClient.SqlConnection
        }
    }
}