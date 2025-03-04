using IMS_API.Models;
using System.Data;
using Microsoft.Data.SqlClient;
using IMS_API.Utilities;

namespace IMS_API.Repositories
{
    public interface ISupplierRepository
    {
        Task<string> CreateSupplier(CreateSupplierDto supplierDto);
        Task<string> UpdateSupplier(UpdateSupplierDto supplierDto);
        Task<SupplierModel> GetSupplierById(int supplierId);
        Task<List<SupplierModel>> GetAllSuppliers(int pageNumber, int pageSize);
    }
    public class SupplierRepository : ISupplierRepository
    {
        private readonly IDatabaseConnectionProvider _connectionProvider;

        public SupplierRepository(IDatabaseConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
        }

        public async Task<string> CreateSupplier(CreateSupplierDto supplierDto)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spCreateSupplier", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@Name", supplierDto.Name);
                        command.Parameters.AddWithValue("@Contact1", supplierDto.Contact1);
                        command.Parameters.AddWithValue("@Contact2", supplierDto.Contact2);
                        command.Parameters.AddWithValue("@Contact3", supplierDto.Contact3);
                        command.Parameters.AddWithValue("@Contact4", supplierDto.Contact4);
                        command.Parameters.AddWithValue("@Address1", supplierDto.Address1);
                        command.Parameters.AddWithValue("@Address2", supplierDto.Address2);
                        command.Parameters.AddWithValue("@City", supplierDto.City);

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
                AppLogic.LogException(nameof(CreateSupplier), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }

        public async Task<string> UpdateSupplier(UpdateSupplierDto supplierDto)
        {
            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spUpdateSupplier", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@SupplierID", supplierDto.SupplierID);
                        command.Parameters.AddWithValue("@Name", supplierDto.Name);
                        command.Parameters.AddWithValue("@Contact1", supplierDto.Contact1);
                        command.Parameters.AddWithValue("@Contact2", supplierDto.Contact2);
                        command.Parameters.AddWithValue("@Contact3", supplierDto.Contact3);
                        command.Parameters.AddWithValue("@Contact4", supplierDto.Contact4);
                        command.Parameters.AddWithValue("@Address1", supplierDto.Address1);
                        command.Parameters.AddWithValue("@Address2", supplierDto.Address2);
                        command.Parameters.AddWithValue("@City", supplierDto.City);
                        command.Parameters.AddWithValue("@IsActive", supplierDto.IsActive);

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
                AppLogic.LogException(nameof(UpdateSupplier), ex.Message, ex.StackTrace);
                return "Failure";
            }
        }
        public async Task<SupplierModel> GetSupplierById(int supplierId)
        {
            var supplier = new SupplierModel();
            var details = new List<SupplierDetailModel>();

            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spGetSupplierById", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@SupplierID", supplierId);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            // Fetch Supplier
                            if (await reader.ReadAsync())
                            {
                                supplier.SupplierID = (int)reader["SupplierID"];
                                supplier.Name = reader["SupplierName"]?.ToString();
                                supplier.IsActive = (bool)reader["IsActive"];
                                supplier.CreatedOn = (DateTime)reader["CreatedOn"];
                                supplier.UpdatedOn = (DateTime)reader["UpdatedOn"];
                            }

                            // Move to next result set for SupplierDetails
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    details.Add(new SupplierDetailModel
                                    {
                                        SDID = (int)reader["SDID"],
                                        SupplierID = (int)reader["SupplierID"],
                                        Name = reader["Name"]?.ToString(),
                                        Contact1 = reader["Contact1"]?.ToString(),
                                        Contact2 = reader["Contact2"]?.ToString(),
                                        Contact3 = reader["Contact3"]?.ToString(),
                                        Contact4 = reader["Contact4"]?.ToString(),
                                        Address1 = reader["Address1"]?.ToString(),
                                        Address2 = reader["Address2"]?.ToString(),
                                        City = reader["City"]?.ToString(),
                                        CreatedOn = (DateTime)reader["CreatedOn"],
                                        UpdatedOn = (DateTime)reader["UpdatedOn"]
                                    });
                                }
                            }
                        }
                    }
                }

                supplier.Details = details; // Attach details to the supplier
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(GetSupplierById), ex.Message, ex.StackTrace);
                throw new Exception("An error occurred while fetching the supplier.", ex);
            }

            return supplier;
        }

        public async Task<List<SupplierModel>> GetAllSuppliers(int pageNumber, int pageSize)
        {
            var suppliers = new List<SupplierModel>();

            try
            {
                using (var connection = (SqlConnection)_connectionProvider.CreateConnection())
                {
                    using (var command = new SqlCommand("spGetAllSuppliers", (SqlConnection)connection))
                    {
                        command.CommandType = CommandType.StoredProcedure;
                        command.Parameters.AddWithValue("@PageNumber", pageNumber);
                        command.Parameters.AddWithValue("@PageSize", pageSize);

                        await connection.OpenAsync();

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var supplierDetails = new Dictionary<int, List<SupplierDetailModel>>();

                            // Fetch suppliers
                            while (await reader.ReadAsync())
                            {
                                var supplierId = (int)reader["SupplierID"];
                                suppliers.Add(new SupplierModel
                                {
                                    SupplierID = supplierId,
                                    Name = reader["SupplierName"]?.ToString(),
                                    IsActive = (bool)reader["IsActive"],
                                    CreatedOn = (DateTime)reader["CreatedOn"],
                                    UpdatedOn = (DateTime)reader["UpdatedOn"]
                                });

                                supplierDetails[supplierId] = new List<SupplierDetailModel>();
                            }

                            // Move to next result set for SupplierDetails
                            if (await reader.NextResultAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var supplierId = (int)reader["SupplierID"];
                                    if (supplierDetails.ContainsKey(supplierId))
                                    {
                                        supplierDetails[supplierId].Add(new SupplierDetailModel
                                        {
                                            SDID = (int)reader["SDID"],
                                            SupplierID = supplierId,
                                            Name = reader["Name"]?.ToString(),
                                            Contact1 = reader["Contact1"]?.ToString(),
                                            Contact2 = reader["Contact2"]?.ToString(),
                                            Contact3 = reader["Contact3"]?.ToString(),
                                            Contact4 = reader["Contact4"]?.ToString(),
                                            Address1 = reader["Address1"]?.ToString(),
                                            Address2 = reader["Address2"]?.ToString(),
                                            City = reader["City"]?.ToString(),
                                            CreatedOn = (DateTime)reader["CreatedOn"],
                                            UpdatedOn = (DateTime)reader["UpdatedOn"]
                                        });
                                    }
                                }
                            }

                            // Attach details to the respective suppliers
                            foreach (var supplier in suppliers)
                            {
                                if (supplierDetails.ContainsKey(supplier.SupplierID))
                                {
                                    supplier.Details = supplierDetails[supplier.SupplierID];
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                AppLogic.LogException(nameof(GetAllSuppliers), ex.Message, ex.StackTrace);
                throw new Exception("An error occurred while fetching suppliers.", ex);
            }

            return suppliers;
        }
        //

    }
}
