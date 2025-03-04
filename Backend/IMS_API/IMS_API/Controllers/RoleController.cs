using IMS_API.Models;
using IMS_API.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoleController : ControllerBase
    {
        private readonly IRoleRepository _roleRepository;

        public RoleController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] CreateOrUpdateRoleDto roleDto)
        {
            var result = await _roleRepository.CreateRole(roleDto.RoleName);
            if (result == "Success")
                return Ok(new { Success = true, Message = "Role created successfully." });
            if (result == "Role Exists")
                return Conflict(new { Success = false, Message = "Role already exists." });
            return StatusCode(500, new { Success = false, Message = "Failed to create role." });
        }

        [HttpPut("update/{roleId}")]
        public async Task<IActionResult> UpdateRole(int roleId, [FromBody] CreateOrUpdateRoleDto roleDto)
        {
            var result = await _roleRepository.UpdateRole(roleId, roleDto);
            if (result == "Success")
                return Ok(new { Success = true, Message = "Role updated successfully." });
            if (result == "Invalid Role")
                return NotFound(new { Success = false, Message = "Role not found." });
            return StatusCode(500, new { Success = false, Message = "Failed to update role." });
        }

        [HttpGet("{roleId}")]
        public async Task<IActionResult> GetRoleById(int roleId)
        {
            var role = await _roleRepository.GetRoleById(roleId);
            if (role.RoleID == 0)
                return NotFound(new { Success = false, Message = "Role not found." });
            return Ok(role);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllRoles(int pageNumber = 1, int pageSize = 10)
        {
            var (roles, totalCount) = await _roleRepository.GetAllRoles(pageNumber, pageSize);
            return Ok(new { Success = true, Data = roles, TotalCount = totalCount });
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRoleToUser(int userId, int roleId)
        {
            var result = await _roleRepository.AssignRoleToUser(userId, roleId);
            if (result == "Success")
                return Ok(new { Success = true, Message = "Role assigned to user successfully." });
            if (result == "Invalid User")
                return BadRequest(new { Success = false, Message = "User ID is invalid." });
            if (result == "Invalid Role")
                return BadRequest(new { Success = false, Message = "Role ID is invalid." });
            if (result == "Exists")
                return Conflict(new { Success = false, Message = "User already has this role assigned." });
            return StatusCode(500, new { Success = false, Message = "Failed to assign role." });
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId)
        {
            var result = await _roleRepository.RemoveRoleFromUser(userId, roleId);
            if (result == "Success")
                return Ok(new { Success = true, Message = "Role removed from user successfully." });
            if (result == "Not Assigned")
                return BadRequest(new { Success = false, Message = "User does not have this role." });
            return StatusCode(500, new { Success = false, Message = "Failed to remove role." });
        }
    }
}
