using IMS_API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;
using IMS_API.Utilities;

namespace IMS_API.Repositories
{
    public interface IUserRepository
    {
        string CreateUser(UserModel user);
        string UpdateUser(UserModel user);
        UserModel GetUserById(int userId);
        List<UserModel> GetAllActiveUsers(int pageNumber, int pageSize);
    }

    public interface IDatabaseConnectionProvider
    {
        IDbConnection CreateConnection();
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
            var connection = new SqlConnection(_context.ConnectionString);
            connection.ConnectionString += ";Pooling=true;Min Pool Size=5;Max Pool Size=50;";
            return connection;
        }
    }

    public class UserRepository : IUserRepository
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public UserRepository(IDatabaseConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public string CreateUser(UserModel user)
        {
            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spCreateUser", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar) { Value = user.FullName });
                        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email });
                        command.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar) { Value = user.Password });
                        command.Parameters.Add(new SqlParameter("@RoleID", SqlDbType.Int) { Value = user.RoleID });
                        command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = user.IsActive });

                        var statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 10)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(statusParam);

                        connection.Open();
                        command.ExecuteNonQuery();

                        return statusParam.Value?.ToString() ?? "Failure";
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(CreateUser), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }

        public string UpdateUser(UserModel user)
        {
            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spUpdateUser", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = user.UserID });
                        command.Parameters.Add(new SqlParameter("@FullName", SqlDbType.NVarChar) { Value = user.FullName });
                        command.Parameters.Add(new SqlParameter("@Email", SqlDbType.NVarChar) { Value = user.Email });
                        command.Parameters.Add(new SqlParameter("@Password", SqlDbType.NVarChar) { Value = user.Password });
                        command.Parameters.Add(new SqlParameter("@RoleID", SqlDbType.Int) { Value = user.RoleID });
                        command.Parameters.Add(new SqlParameter("@IsActive", SqlDbType.Bit) { Value = user.IsActive });

                        var statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 10)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(statusParam);

                        connection.Open();
                        command.ExecuteNonQuery();

                        return statusParam.Value?.ToString() ?? "Failure";
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(UpdateUser), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }

        public UserModel GetUserById(int userId)
        {
            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spGetUserById", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@UserID", SqlDbType.Int) { Value = userId });

                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new UserModel
                                {
                                    UserID = (int)reader["UserID"],
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Password = reader["Password"].ToString(),
                                    RoleID = (int)reader["RoleID"],
                                    IsActive = (bool)reader["IsActive"]
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

        public List<UserModel> GetAllActiveUsers(int pageNumber, int pageSize)
        {
            var users = new List<UserModel>();
            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spGetAllActiveUsers", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                users.Add(new UserModel
                                {
                                    UserID = (int)reader["UserID"],
                                    FullName = reader["FullName"].ToString(),
                                    Email = reader["Email"].ToString(),
                                    Password = reader["Password"].ToString(),
                                    RoleID = (int)reader["RoleID"],
                                    IsActive = (bool)reader["IsActive"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("An error occurred while fetching active users.", ex);
            }

            return users;
        }
    }


}