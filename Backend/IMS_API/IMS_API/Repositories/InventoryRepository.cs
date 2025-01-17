using IMS_API.Models;
using IMS_API.Utilities;
using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.SqlClient;

namespace IMS_API.Repositories
{
    public interface IInventoryRepository
    {
        Task<string> CreateAsync(CreateInventoryItemDto itemDto);
        Task<string> UpdateAsync(UpdateInventoryItemDto itemDto);
        Task<InventoryItemModel> GetByIdAsync(int itemId);
        Task<List<InventoryItemModel>> GetAllAsync(int pageNumber, int pageSize);
    }

    public class InventoryRepository : IInventoryRepository
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public InventoryRepository(IDatabaseConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public async Task<string> CreateAsync(CreateInventoryItemDto itemDto)
        {
            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spCreateInventoryItem", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Name", itemDto.Name);
                        command.Parameters.AddWithValue("@Description", itemDto.Description);
                        command.Parameters.AddWithValue("@CategoryID", itemDto.CategoryID);
                        command.Parameters.AddWithValue("@Quantity", itemDto.Quantity);
                        command.Parameters.AddWithValue("@UnitPrice", itemDto.UnitPrice);

                        var statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 10) { Direction = ParameterDirection.Output };
                        command.Parameters.Add(statusParam);

                        await connection.OpenAsync();
                        await command.ExecuteNonQueryAsync();

                        return statusParam.Value?.ToString() ?? "Failure";
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(CreateAsync), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }

        public string UpdateInventoryItem(UpdateInventoryItemDto itemDto)
        {
            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spUpdateInventoryItem", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.Add(new SqlParameter("@ItemID", SqlDbType.Int) { Value = itemDto.ItemID });
                        command.Parameters.Add(new SqlParameter("@Name", SqlDbType.NVarChar) { Value = itemDto.Name });
                        command.Parameters.Add(new SqlParameter("@Description", SqlDbType.NVarChar) { Value = itemDto.Description });
                        command.Parameters.Add(new SqlParameter("@CategoryID", SqlDbType.Int) { Value = itemDto.CategoryID });
                        command.Parameters.Add(new SqlParameter("@Quantity", SqlDbType.Int) { Value = itemDto.Quantity });
                        command.Parameters.Add(new SqlParameter("@UnitPrice", SqlDbType.Decimal) { Value = itemDto.UnitPrice });

                        var statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 10)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(statusParam);

                        connection.Open();
                        command.ExecuteNonQuery();

                        return statusParam.Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(UpdateInventoryItem), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }

        public InventoryItemModel GetInventoryItemById(int itemId)
        {
            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spGetInventoryItemById", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@ItemID", SqlDbType.Int) { Value = itemId });

                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                return new InventoryItemModel
                                {
                                    ItemID = (int)reader["ItemID"],
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    CategoryID = (int)reader["CategoryID"],
                                    Category = reader["Category"].ToString(),
                                    Quantity = (int)reader["Quantity"],
                                    UnitPrice = (decimal)reader["UnitPrice"],
                                    IsActive = (bool)reader["IsActive"]
                                };
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(GetInventoryItemById), ex.Message, ex.StackTrace);
            }

            return null;
        }

        public List<InventoryItemModel> GetAllInventoryItems(int pageNumber, int pageSize)
        {
            var items = new List<InventoryItemModel>();
            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spGetAllInventoryItems", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.Add(new SqlParameter("@PageNumber", SqlDbType.Int) { Value = pageNumber });
                        command.Parameters.Add(new SqlParameter("@PageSize", SqlDbType.Int) { Value = pageSize });

                        connection.Open();

                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                items.Add(new InventoryItemModel
                                {
                                    ItemID = (int)reader["ItemID"],
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"].ToString(),
                                    CategoryID = (int)reader["CategoryID"],
                                    Category = reader["Category"].ToString(),
                                    Quantity = (int)reader["Quantity"],
                                    UnitPrice = (decimal)reader["UnitPrice"],
                                    IsActive = (bool)reader["IsActive"]
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(GetAllInventoryItems), ex.Message, ex.StackTrace);
                throw;
            }

            return items;
        }
    }
}
