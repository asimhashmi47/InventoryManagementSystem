using IMS_API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace IMS_API.Repositories
{
    public interface IUserRepository
    {
        void CreateUser(User user);
        User GetUserById(int userId);
        void UpdateUser(User user);
        List<User> GetAllActiveUsers();
    }

    public interface IDatabaseConnectionProvider
    {
        IDbConnection CreateConnection(); // Use IDbConnection interface
    }

    public class DatabaseConnectionProvider : IDatabaseConnectionProvider
    {
        private readonly DatabaseContext _context;

        public DatabaseConnectionProvider(DatabaseContext context)
        {
            _context = context;
        }

        public IDbConnection CreateConnection()
        {
            // Returning a SqlConnection from Microsoft.Data.SqlClient
            return _context.CreateConnection();
        }
    }

    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public UserRepository(IDatabaseConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public void CreateUser(User user)
        {
            if (user == null)
            {
                throw new ArgumentNullException(nameof(user), "User object cannot be null.");
            }

            using (var dbConnection = _connectionProvider.CreateConnection())
            {
                using (var cmd = new SqlCommand("spCreateUser", (SqlConnection)dbConnection)) // Ensure casting to Microsoft.Data.SqlClient.SqlConnection
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    cmd.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar) { Value = user.FullName });
                    cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email });
                    cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar) { Value = user.Password });
                    cmd.Parameters.Add(new SqlParameter("@Role", SqlDbType.NVarChar) { Value = user.Role });
                    cmd.Parameters.Add(new SqlParameter("@CreatedOn", SqlDbType.DateTime) { Value = DateTime.Now });
                    cmd.Parameters.Add(new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now });

                    try
                    {
                        dbConnection.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception("An error occurred while creating the user.", ex);
                    }
                }
            }
        }

        public User GetUserById(int userId)
        {
            try
            {
                using (var dbConnection = _connectionProvider.CreateConnection())
                {
                    using (var cmd = new SqlCommand("spGetUserById", (SqlConnection)dbConnection)) // Ensure casting to Microsoft.Data.SqlClient.SqlConnection
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = userId });

                        dbConnection.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new User
                                {
                                    UserID = (int)reader["UserID"],
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Password = reader["Password"].ToString(),
                                    Role = reader["Role"].ToString(),
                                    IsActive = (bool)reader["IsActive"],
                                    CreatedOn = (DateTime)reader["CreatedOn"],
                                    UpdatedOn = (DateTime)reader["UpdatedOn"]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching the user by ID.", ex);
            }
            return null;
        }

        public void UpdateUser(User user)
        {
            try
            {
                using (var dbConnection = _connectionProvider.CreateConnection())
                {
                    using (var cmd = new SqlCommand("spUpdateUser", (SqlConnection)dbConnection)) // Ensure casting to Microsoft.Data.SqlClient.SqlConnection
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = user.UserID });
                        cmd.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar) { Value = user.FullName });
                        cmd.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email });
                        cmd.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar) { Value = user.Password });
                        cmd.Parameters.Add(new SqlParameter("@Role", SqlDbType.NVarChar) { Value = user.Role });
                        cmd.Parameters.Add(new SqlParameter("@UpdatedOn", SqlDbType.DateTime) { Value = DateTime.Now });

                        dbConnection.Open();
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while updating the user.", ex);
            }
        }

        public List<User> GetAllActiveUsers()
        {
            var users = new List<User>();

            try
            {
                using (var dbConnection = _connectionProvider.CreateConnection())
                {
                    using (var cmd = new SqlCommand("spGetAllActiveUsers", (SqlConnection)dbConnection)) // Ensure casting to Microsoft.Data.SqlClient.SqlConnection
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        dbConnection.Open();

                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new User
                                {
                                    UserID = (int)reader["UserID"],
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Password = reader["Password"].ToString(),
                                    Role = reader["Role"].ToString(),
                                    IsActive = (bool)reader["IsActive"],
                                    CreatedOn = (DateTime)reader["CreatedOn"],
                                    UpdatedOn = (DateTime)reader["UpdatedOn"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching all active users.", ex);
            }

            return users;
        }
    }
}
