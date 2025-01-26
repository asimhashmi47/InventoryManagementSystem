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
        // Inventory Item
        Task<string> CreateInventoryItem(CreateInventoryItemDto itemDto);
        Task<string> UpdateInventoryItem(UpdateInventoryItemDto itemDto);
        Task<InventoryItemModel> GetInventoryItemById(int itemId);
        Task<List<InventoryItemModel>> GetAllInventoryItems(int pageNumber, int pageSize);
        // Stock
        Task<string> StockIn(int itemId, int quantity, string notes);
        Task<string> StockOut(int itemId, int quantity, string notes);
        // Low stock alert
        Task<List<LowStockItemModel>> GetLowStockItems();
        // Inventory Search
        Task<List<SearchResultModel>> SearchInventoryItems(string searchTerm, int pageNumber, int pageSize);
        // Inventory Batch
        Task<string> AddBatch(CreateBatchDto batchDto);
        Task<string> UpdateBatch(UpdateBatchDto batchDto);
        Task<List<InventoryBatchModel>> GetBatchesByItem(int itemId, int pageNumber, int pageSize);
        Task<string> DeleteBatch(int batchId);
        // Transfer Inventory
        Task<string> TransferInventory(string itemName, int quantity, string location, int userId, string notes);
    }

    public class InventoryRepository : IInventoryRepository
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public InventoryRepository(IDatabaseConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public async Task<string> CreateInventoryItem(CreateInventoryItemDto itemDto)
        {
            if (itemDto == null)
                throw new ArgumentNullException(nameof(itemDto), "Item data cannot be null.");

            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("spCreateInventoryItem", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@Name", itemDto.Name ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Description", itemDto.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CategoryID", itemDto.CategoryID);
                        command.Parameters.AddWithValue("@Quantity", itemDto.Quantity);
                        command.Parameters.AddWithValue("@UnitPrice", itemDto.UnitPrice);

                        var statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 10)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(statusParam);

                        await command.ExecuteNonQueryAsync();

                        return statusParam.Value?.ToString() ?? "Failure";
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(CreateInventoryItem), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }
        //
        public async Task<string> UpdateInventoryItem(UpdateInventoryItemDto itemDto)
        {
            if (itemDto == null || itemDto.ItemID <= 0)
                throw new ArgumentException("Invalid inventory item data.", nameof(itemDto));

            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection()) // Explicitly cast to SqlConnection
                {
                    await connection.OpenAsync(); // Use OpenAsync for non-blocking connection

                    using (var command = new SqlCommand("spUpdateInventoryItem", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        // Add parameters
                        command.Parameters.AddWithValue("@ItemID", itemDto.ItemID);
                        command.Parameters.AddWithValue("@Name", itemDto.Name ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@Description", itemDto.Description ?? (object)DBNull.Value);
                        command.Parameters.AddWithValue("@CategoryID", itemDto.CategoryID);
                        command.Parameters.AddWithValue("@Quantity", itemDto.Quantity);
                        command.Parameters.AddWithValue("@UnitPrice", itemDto.UnitPrice);

                        // Output parameter for status
                        var statusParam = new SqlParameter("@Status", SqlDbType.NVarChar, 10)
                        {
                            Direction = ParameterDirection.Output
                        };
                        command.Parameters.Add(statusParam);

                        // Execute the command asynchronously
                        await command.ExecuteNonQueryAsync();

                        // Return the status output parameter
                        return statusParam.Value?.ToString() ?? "Failure";
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                AppLogic.LogException(nameof(UpdateInventoryItem), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }

        public async Task<InventoryItemModel> GetInventoryItemById(int itemId)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("spGetInventoryItemById", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ItemID", itemId);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
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

        public async Task<List<InventoryItemModel>> GetAllInventoryItems(int pageNumber, int pageSize)
        {
            var items = new List<InventoryItemModel>();

            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    await connection.OpenAsync();

                    using (var command = new SqlCommand("spGetAllInventoryItems", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PageNumber", pageNumber);
                        command.Parameters.AddWithValue("@PageSize", pageSize);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
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
        //
        // stock in
        public async Task<string> StockIn(int itemId, int quantity, string notes)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spStockIn", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ItemID", itemId);
                        command.Parameters.AddWithValue("@Quantity", quantity);
                        command.Parameters.AddWithValue("@Notes", notes ?? (object)DBNull.Value);

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
                AppLogic.LogException(nameof(StockIn), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }
        //
        // stock out
        public async Task<string> StockOut(int itemId, int quantity, string notes)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spStockOut", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ItemID", itemId);
                        command.Parameters.AddWithValue("@Quantity", quantity);
                        command.Parameters.AddWithValue("@Notes", notes ?? (object)DBNull.Value);

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
                AppLogic.LogException(nameof(StockOut), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }
        //
        // Low Stock Alerts
        public async Task<List<LowStockItemModel>> GetLowStockItems()
        {
            var lowStockItems = new List<LowStockItemModel>();

            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spGetLowStockItems", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        connection.Open();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                lowStockItems.Add(new LowStockItemModel
                                {
                                    ItemID = (int)reader["ItemID"],
                                    Name = reader["Name"].ToString(),
                                    Description = reader["Description"]?.ToString(),
                                    Category = reader["Category"].ToString(),
                                    TotalQuantity = reader["TotalQuantity"] != DBNull.Value ? (int)reader["TotalQuantity"] : 0,
                                    InventoryThreshold = (int)reader["InventoryThreshold"], // Updated
                                    UnitPrice = (decimal)reader["UnitPrice"],
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(GetLowStockItems), ex.Message, ex.StackTrace);
                throw new Exception("An error occurred while fetching low stock items.", ex);
            }
            return lowStockItems;
        }
        //
        public async Task<List<SearchResultModel>> SearchInventoryItems(string searchTerm, int pageNumber, int pageSize)
        {
            var searchResults = new List<SearchResultModel>();

            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spSearchInventoryItems", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@SearchTerm", (object)searchTerm ?? DBNull.Value);
                        command.Parameters.AddWithValue("@PageNumber", pageNumber);
                        command.Parameters.AddWithValue("@PageSize", pageSize);

                        connection.Open();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                searchResults.Add(new SearchResultModel
                                {
                                    ItemID = (int)reader["ItemID"],
                                    ItemName = reader["ItemName"].ToString(),
                                    Description = reader["Description"]?.ToString(),
                                    CategoryID = (int)reader["CategoryID"],
                                    CategoryName = reader["CategoryName"].ToString(),
                                    InitialQuantity = (int)reader["InitialQuantity"],
                                    TotalQuantity = (int)reader["TotalQuantity"],
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
                AppLogic.LogException(nameof(SearchInventoryItems), ex.Message, ex.StackTrace);
                throw new Exception("An error occurred while searching inventory items.", ex);
            }

            return searchResults;
        }
        // 
        // Inventory Batch
        public async Task<string> AddBatch(CreateBatchDto batchDto)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spAddBatch", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@ItemID", batchDto.ItemID);
                        command.Parameters.AddWithValue("@BatchNumber", batchDto.BatchNumber);
                        command.Parameters.AddWithValue("@ManufactureDate", (object)batchDto.ManufactureDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ExpiryDate", (object)batchDto.ExpiryDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Quantity", batchDto.Quantity);

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
                AppLogic.LogException(nameof(AddBatch), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }

        public async Task<string> UpdateBatch(UpdateBatchDto batchDto)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spUpdateBatch", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@BatchID", batchDto.BatchID);
                        command.Parameters.AddWithValue("@BatchNumber", batchDto.BatchNumber);
                        command.Parameters.AddWithValue("@ManufactureDate", (object)batchDto.ManufactureDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@ExpiryDate", (object)batchDto.ExpiryDate ?? DBNull.Value);
                        command.Parameters.AddWithValue("@Quantity", batchDto.Quantity);

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
                AppLogic.LogException(nameof(UpdateBatch), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }

        public async Task<List<InventoryBatchModel>> GetBatchesByItem(int itemId, int pageNumber, int pageSize)
        {
            var batches = new List<InventoryBatchModel>();
            try
            {
                using (var connection = _connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spGetBatchesByItem", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@ItemID", itemId);
                        command.Parameters.AddWithValue("@PageNumber", pageNumber);
                        command.Parameters.AddWithValue("@PageSize", pageSize);

                        connection.Open();
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (reader.Read())
                            {
                                batches.Add(new InventoryBatchModel
                                {
                                    BatchID = (int)reader["BatchID"],
                                    ItemID = (int)reader["ItemID"],
                                    ItemName = reader["ItemName"].ToString(),
                                    BatchNumber = reader["BatchNumber"].ToString(),
                                    ManufactureDate = reader["ManufactureDate"] != DBNull.Value ? (DateTime)reader["ManufactureDate"] : (DateTime?)null,
                                    ExpiryDate = reader["ExpiryDate"] != DBNull.Value ? (DateTime)reader["ExpiryDate"] : (DateTime?)null,
                                    Quantity = (int)reader["Quantity"],
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
                AppLogic.LogException(nameof(GetBatchesByItem), ex.Message, ex.StackTrace);
                throw new Exception("An error occurred while fetching batches.", ex);
            }

            return batches;
        }

        public async Task<string> DeleteBatch(int batchId)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spDeleteBatch", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@BatchID", batchId);

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
                AppLogic.LogException(nameof(DeleteBatch), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }
        //
        // Transfer Inventory
        public async Task<string> TransferInventory(string itemName, int quantity, string location, int userId, string notes)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spTransferInventory", connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;

                        command.Parameters.AddWithValue("@ItemName", itemName);
                        command.Parameters.AddWithValue("@Quantity", quantity);
                        command.Parameters.AddWithValue("@Location", location);
                        command.Parameters.AddWithValue("@UserID", userId);
                        command.Parameters.AddWithValue("@Notes", notes ?? (object)DBNull.Value);

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
                AppLogic.LogException(nameof(TransferInventory), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }
        //

    }
}
