using IMS_API.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Data.SqlClient;
using IMS_API.Utilities;

namespace IMS_API.Repositories
{
    public interface IRoleRepository
    {
        Task<string> CreateRole(string roleName);
        Task<string> UpdateRole(int roleId, CreateOrUpdateRoleDto roleDto);
        Task<RoleModel> GetRoleById(int roleId);
        Task<(List<RoleModel> Roles, int TotalCount)> GetAllRoles(int pageNumber, int pageSize);
        Task<string> AssignRoleToUser(int userId, int roleId);
        Task<string> RemoveRoleFromUser(int userId, int roleId);
    }
    //
    public class RoleRepository : IRoleRepository
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public RoleRepository(IDatabaseConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        // 1. Create Role
        public async Task<string> CreateRole(string roleName)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spCreateRole", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@RoleName", roleName);

                        var statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 10)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(statusParam);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        return statusParam.Value?.ToString() ?? "Failure";
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(CreateRole), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }

        // 2. Update Role
        public async Task<string> UpdateRole(int roleId, CreateOrUpdateRoleDto roleDto)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spUpdateRole", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@RoleID", roleId);
                        command.Parameters.AddWithValue("@RoleName", roleDto.RoleName);
                        command.Parameters.AddWithValue("@IsActive", roleDto.IsActive);

                        var statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 10)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(statusParam);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        return statusParam.Value?.ToString() ?? "Failure";
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(UpdateRole), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }

        // 3. Get Role By ID
        public async Task<RoleModel> GetRoleById(int roleId)
        {
            var role = new RoleModel();

            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spGetRoleById", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@RoleID", roleId);

                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                role.RoleID = (int)reader["RoleID"];
                                role.RoleName = reader["RoleName"]?.ToString();
                                role.IsActive = (bool)reader["IsActive"];
                                role.CreatedOn = (DateTime)reader["CreatedOn"];
                                role.UpdatedOn = (DateTime)reader["UpdatedOn"];
                            }
                        }
                    }
                }
                return role;
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(GetRoleById), ex.Message, ex.StackTrace);
                throw;
            }
        }

        // 4. Get All Roles (with pagination)
        public async Task<(List<RoleModel> Roles, int TotalCount)> GetAllRoles(int pageNumber, int pageSize)
        {
            var roles = new List<RoleModel>();
            int totalCount = 0;

            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spGetAllRoles", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PageNumber", pageNumber);
                        command.Parameters.AddWithValue("@PageSize", pageSize);

                        await connection.OpenAsync();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Roles result set
                            while (await reader.ReadAsync())
                            {
                                roles.Add(new RoleModel
                                {
                                    RoleID = (int)reader["RoleID"],
                                    RoleName = reader["RoleName"]?.ToString(),
                                    IsActive = (bool)reader["IsActive"],
                                    CreatedOn = (DateTime)reader["CreatedOn"],
                                    UpdatedOn = (DateTime)reader["UpdatedOn"]
                                });
                            }

                            // Move to next result set for total count
                            if (await reader.NextResultAsync() && await reader.ReadAsync())
                            {
                                totalCount = (int)reader["TotalCount"];
                            }
                        }
                    }
                }
                return (roles, totalCount);
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(GetAllRoles), ex.Message, ex.StackTrace);
                throw;
            }
        }

        // 5. Assign Role to User
        public async Task<string> AssignRoleToUser(int userId, int roleId)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spAssignRoleToUser", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.Parameters.AddWithValue("@RoleID", roleId);

                        var statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 10)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(statusParam);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        return statusParam.Value?.ToString() ?? "Failure";
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(AssignRoleToUser), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }

        // 6. Remove Role from User
        public async Task<string> RemoveRoleFromUser(int userId, int roleId)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spRemoveRoleFromUser", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.Parameters.AddWithValue("@RoleID", roleId);

                        var statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 10)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(statusParam);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        return statusParam.Value?.ToString() ?? "Failure";
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(RemoveRoleFromUser), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }
    }
}
