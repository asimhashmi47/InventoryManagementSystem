

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

}
