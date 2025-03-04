using IMS_API.Models;
using IMS_API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplierController : Controller
    {
        private readonly ISupplierRepository _supplierRepository;

        public SupplierController(ISupplierRepository supplierRepository)
        {
            _supplierRepository = supplierRepository;
        }

        [HttpPost("CreateSupplier")]
        public async Task<IActionResult> CreateSupplier(CreateSupplierDto supplierDto)
        {
            var result = await _supplierRepository.CreateSupplier(supplierDto);
            if (result == "Success")
                return Ok(new { Success = true, Message = "Supplier created successfully." });

            return BadRequest(new { Success = false, Message = "Failed to create supplier." });
        }

        [HttpPut("UpdateSupplier")]
        public async Task<IActionResult> UpdateSupplier(UpdateSupplierDto supplierDto)
        {
            var result = await _supplierRepository.UpdateSupplier(supplierDto);
            if (result == "Success")
                return Ok(new { Success = true, Message = "Supplier updated successfully." });

            return BadRequest(new { Success = false, Message = "Failed to update supplier." });
        }

        [HttpGet("GetSupplierById")]
        public async Task<IActionResult> GetSupplierById(int id)
        {
            var supplier = await _supplierRepository.GetSupplierById(id);
            if (supplier == null)
                return NotFound(new { Success = false, Message = "Supplier not found." });

            return Ok(new { Success = true, Data = supplier });
        }

        [HttpGet("GetAllSuppliers")]
        public async Task<IActionResult> GetAllSuppliers(int pageNumber = 1, int pageSize = 10)
        {
            var suppliers = await _supplierRepository.GetAllSuppliers(pageNumber, pageSize);
            return Ok(new { Success = true, Data = suppliers });
        }
        //

    }
}
