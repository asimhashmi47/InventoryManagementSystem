using IMS_API.Models;
using IMS_API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        [HttpPost("CreateUser")]
        public IActionResult CreateUser([FromBody] CreateUserDto userDto)
        {
            if (userDto == null) return BadRequest("User data is required.");

            // Map CreateUserDto to UserModel
            var userModel = new UserModel
            {
                FullName = userDto.FullName,
                Email = userDto.Email,
                Password = userDto.Password,
                RoleID = userDto.RoleID,
                IsActive = userDto.IsActive
            };

            var status = _userRepository.CreateUser(userModel);
            if (status == "Success")
            {
                return Ok("User created successfully.");
            }
                
            return BadRequest(status); // Return relevant failure message
        }

        [HttpPut("UpdateUser")]
        public IActionResult UpdateUser([FromBody] UpdateUserDto userDto)
        {
            if (userDto == null) return BadRequest("User data is required.");

            // Map UpdateUserDto to UserModel
            var userModel = new UserModel
            {
                UserID = userDto.UserID,
                FullName = userDto.FullName,
                Email = userDto.Email,
                Password = userDto.Password,
                RoleID = userDto.RoleID,
                IsActive = userDto.IsActive
            };

            var status = _userRepository.UpdateUser(userModel);
            if (status == "Success")
                return Ok("User updated successfully.");
            return BadRequest(status); // Return relevant failure message
        }

        [HttpGet("{id}")]
        public ActionResult<UserModel> GetUserById(int id)
        {
            var user = _userRepository.GetUserById(id);
            if (user == null)
            {
                return NotFound("User not found.");
            }
                
            return Ok(user);
        }

        [HttpGet("GetAllActiveUsers")]
        public ActionResult<List<UserModel>> GetAllActiveUsers(int pageNumber, int pageSize)
        {
            try
            {
                var users = _userRepository.GetAllActiveUsers(pageNumber, pageSize);
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "An error occurred while fetching active users.", Details = ex.Message });
            }
        }
    }
}
