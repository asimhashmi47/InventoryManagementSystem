using IMS_API.Models;
using IMS_API.Repositories;
using IMS_API.Utilities;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InventoryItemsController : Controller
    {
        private readonly IInventoryRepository _inventoryRepository;

        public InventoryItemsController(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository ?? throw new ArgumentNullException(nameof(inventoryRepository));
        }

        // POST: api/InventoryItems/create
        [HttpPost("CreateInventoryItem")]
        public async Task<IActionResult> CreateInventoryItem([FromBody] CreateInventoryItemDto itemDto)
        {
            if (itemDto == null)
                return BadRequest(new { Success = false, Message = "Invalid inventory item data." });

            // Await the asynchronous repository method
            var result = await _inventoryRepository.CreateInventoryItem(itemDto);

            if (result == "Success")
                return Ok(new { Success = true, Message = "Inventory item created successfully." });

            return BadRequest(new { Success = false, Message = "Failed to create inventory item." });
        }

        // PUT: api/InventoryItems/update
        [HttpPut("UpdateInventoryItem")]
        public async Task<IActionResult> UpdateInventoryItem([FromBody] UpdateInventoryItemDto itemDto)
        {
            if (itemDto == null || itemDto.ItemID <= 0)
                return BadRequest(new { Success = false, Message = "Invalid inventory item data." });

            // Await the asynchronous repository method
            var result = await _inventoryRepository.UpdateInventoryItem(itemDto);

            if (result == "Success")
                return Ok(new { Success = true, Message = "Inventory item updated successfully." });

            return BadRequest(new { Success = false, Message = "Failed to update inventory item." });
        }

        // GET: api/InventoryItems/{id}
        [HttpGet("{id}")]
        public IActionResult GetInventoryItemById(int id)
        {
            if (id <= 0)
                return BadRequest(new { Success = false, Message = "Invalid item ID." });

            var item = _inventoryRepository.GetInventoryItemById(id);
            if (item == null)
                return NotFound(new { Success = false, Message = "Inventory item not found." });

            return Ok(new { Success = true, Data = item });
        }

        // GET: api/InventoryItems/all
        [HttpGet("GetAllInventoryItems")]
        public async Task<IActionResult> GetAllInventoryItems(int pageNumber = 1, int pageSize = 10)
        {
            // Validate page number and page size
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest(new { Success = false, Message = "Invalid pagination parameters." });

            // Enforce a maximum page size limit (optional)
            if (pageSize > 100)
                return BadRequest(new { Success = false, Message = "Page size too large. Max allowed is 100." });

            try
            {
                // Fetch paginated inventory items
                var items = await _inventoryRepository.GetAllInventoryItems(pageNumber, pageSize);

                if (items == null)
                    return NotFound(new { Success = false, Message = "Inventory item not found." });

                return Ok(new { Success = true, Data = items });
            }
            catch (Exception ex)
            {
                // Handle unexpected exceptions and log
                AppLogic.LogException(nameof(GetAllInventoryItems), ex.Message, ex.StackTrace);
                return StatusCode(500, new { Success = false, Message = "An error occurred while fetching inventory items." });
            }
        }
        //
        [HttpPost("Stock-in")]
        public async Task<IActionResult> StockIn([FromBody] StockTransactionDto stockDto)
        {
            if (stockDto == null || stockDto.ItemID <= 0 || stockDto.Quantity <= 0)
                return BadRequest(new { Success = false, Message = "Invalid stock-in data." });

            var result = await _inventoryRepository.StockIn(stockDto.ItemID, stockDto.Quantity, stockDto.Notes);

            if (result == "Success")
                return Ok(new { Success = true, Message = "Stock-in successful." });

            return BadRequest(new { Success = false, Message = result });
        }
        //
        [HttpPost("Stock-out")]
        public async Task<IActionResult> StockOut([FromBody] StockTransactionDto stockDto)
        {
            if (stockDto == null || stockDto.ItemID <= 0 || stockDto.Quantity <= 0)
                return BadRequest(new { Success = false, Message = "Invalid stock-out data." });

            var result = await _inventoryRepository.StockOut(stockDto.ItemID, stockDto.Quantity, stockDto.Notes);

            if (result == "Success")
                return Ok(new { Success = true, Message = "Stock-out successful." });

            return BadRequest(new { Success = false, Message = result });
        }
        //
        [HttpGet("GetLowStockItems")]
        public async Task<IActionResult> GetLowStockItems()
        {
            try
            {
                var lowStockItems = await _inventoryRepository.GetLowStockItems();
                return Ok(new { Success = true, Data = lowStockItems });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while fetching low stock items.", Details = ex.Message });
            }
        }
        //
        [HttpGet("SearchInventoryItems")]
        public async Task<IActionResult> SearchInventoryItems(
        [FromQuery] string searchTerm,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
        {
            try
            {
                var searchResults = await _inventoryRepository.SearchInventoryItems(searchTerm, pageNumber, pageSize);
                return Ok(new { Success = true, Data = searchResults });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred while searching inventory items.", Details = ex.Message });
            }
        }
        //
        [HttpPost("AddBatch")]
        public async Task<IActionResult> AddBatch([FromBody] CreateBatchDto batchDto)
        {
            var result = await _inventoryRepository.AddBatch(batchDto);
            if (result == "Success")
                return Ok(new { Success = true, Message = "Batch added successfully." });

            return BadRequest(new { Success = false, Message = "Failed to add batch." });
        }

        [HttpPut("UpdateBatch")]
        public async Task<IActionResult> UpdateBatch([FromBody] UpdateBatchDto batchDto)
        {
            var result = await _inventoryRepository.UpdateBatch(batchDto);
            if (result == "Success")
                return Ok(new { Success = true, Message = "Batch updated successfully." });

            return BadRequest(new { Success = false, Message = "Failed to update batch." });
        }

        [HttpGet("GetBatchesByItem")]
        public async Task<IActionResult> GetBatchesByItem(int itemId, int pageNumber = 1, int pageSize = 10)
        {
            var batches = await _inventoryRepository.GetBatchesByItem(itemId, pageNumber, pageSize);
            return Ok(new { Success = true, Data = batches });
        }

        [HttpDelete("DeleteBatch")]
        public async Task<IActionResult> DeleteBatch(int batchId)
        {
            var result = await _inventoryRepository.DeleteBatch(batchId);
            if (result == "Success")
                return Ok(new { Success = true, Message = "Batch deleted successfully." });

            return BadRequest(new { Success = false, Message = "Failed to delete batch." });
        }
        //
        [HttpPost("TransferInventory")]
        public async Task<IActionResult> TransferInventory([FromBody] TransferInventoryDto transferDto)
        {
            if (transferDto == null || string.IsNullOrWhiteSpace(transferDto.ItemName) || transferDto.Quantity <= 0)
            {
                return BadRequest(new { Success = false, Message = "Invalid transfer data." });
            }

            var result = await _inventoryRepository.TransferInventory(
                transferDto.ItemName,
                transferDto.Quantity,
                transferDto.Location,
                transferDto.UserID,
                transferDto.Notes
            );

            if (result == "Success")
            {
                return Ok(new { Success = true, Message = "Inventory transfer successful." });
            }
            else if (result == "Insufficient Stock")
            {
                return BadRequest(new { Success = false, Message = "Insufficient stock available." });
            }
            else if (result == "Item Not Found")
            {
                return NotFound(new { Success = false, Message = "Item not found." });
            }
            else
            {
                return StatusCode(500, new { Success = false, Message = "An error occurred during the transfer." });
            }
        }
        //

    }
}
