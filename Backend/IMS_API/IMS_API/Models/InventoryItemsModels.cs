

namespace IMS_API.Models
{
    // This represents the core inventory item entity used in your business logic or database operations.
    public class InventoryItemModel
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public string Category { get; set; } // Optional for display purposes
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO for creating a new inventory item.
    public class CreateInventoryItemDto
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    // DTO for updating inventory item information.
    public class UpdateInventoryItemDto
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
    }
    //
    public class StockTransactionDto
    {
        public int ItemID { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
    }
    //
    public class LowStockItemModel
    {
        public int ItemID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public int TotalQuantity { get; set; }
        public int InventoryThreshold { get; set; }
        public decimal UnitPrice { get; set; }
    }
    //
    public class SearchResultModel
    {
        public int ItemID { get; set; }
        public string ItemName { get; set; }
        public string Description { get; set; }
        public int CategoryID { get; set; }
        public string CategoryName { get; set; }
        public int InitialQuantity { get; set; }
        public int TotalQuantity { get; set; }
        public decimal UnitPrice { get; set; }
        public bool IsActive { get; set; }
    }
    //
    // Core model representing a batch/lot
    public class InventoryBatchModel
    {
        public int BatchID { get; set; }
        public int ItemID { get; set; }
        public string ItemName { get; set; } // Optional for display purposes
        public string BatchNumber { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int Quantity { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    // DTO for creating a batch
    public class CreateBatchDto
    {
        public int ItemID { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int Quantity { get; set; }
    }

    // DTO for updating a batch
    public class UpdateBatchDto
    {
        public int BatchID { get; set; }
        public string BatchNumber { get; set; }
        public DateTime? ManufactureDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public int Quantity { get; set; }
    }
    //
    public class TransferInventoryDto
    {
        public string ItemName { get; set; } // Name of the item to transfer
        public int Quantity { get; set; }   // Quantity to transfer
        public string Location { get; set; } // Destination location
        public int UserID { get; set; }     // User performing the transfer
        public string Notes { get; set; }   // Optional notes
    }
    //

}
