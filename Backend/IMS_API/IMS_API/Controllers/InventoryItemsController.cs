using IMS_API.Models;
using IMS_API.Repositories;
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
        public IActionResult CreateInventoryItem([FromBody] CreateInventoryItemDto itemDto)
        {
            if (itemDto == null)
                return BadRequest(new { Success = false, Message = "Invalid inventory item data." });

            var result = _inventoryRepository.CreateInventoryItem(itemDto);
            if (result == "Success")
                return Ok(new { Success = true, Message = "Inventory item created successfully." });

            return BadRequest(new { Success = false, Message = "Failed to create inventory item." });
        }

        // PUT: api/InventoryItems/update
        [HttpPut("UpdateInventoryItem")]
        public IActionResult UpdateInventoryItem([FromBody] UpdateInventoryItemDto itemDto)
        {
            if (itemDto == null || itemDto.ItemID <= 0)
                return BadRequest(new { Success = false, Message = "Invalid inventory item data." });

            var result = _inventoryRepository.UpdateInventoryItem(itemDto);
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
            if (pageNumber <= 0 || pageSize <= 0)
                return BadRequest(new { Success = false, Message = "Invalid pagination parameters." });

            var items = await _inventoryRepository.GetAllAsync(pageNumber, pageSize);

            return Ok(new { Success = true, Data = items });
        }
    }
}
