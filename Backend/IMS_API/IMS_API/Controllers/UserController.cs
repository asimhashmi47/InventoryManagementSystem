using IMS_API.Models;
using IMS_API.Repositories;
using IMS_API.Utilities;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace IMS_API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableCors("AllowAll")] // Applies the CORS policy named "AllowAll" to this controller
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
        public IActionResult GetAllActiveUsers(int pageNumber, int pageSize)
        {
            try
            {
                // Fetch data from the repository
                var users = _userRepository.GetAllActiveUsers(pageNumber, pageSize);

                if (users == null || users.Count == 0)
                {
                    // Return a response indicating no data found
                    return Ok(new
                    {
                        Success = false,
                        Message = "No active users found.",
                        Data = new List<UserModel>()
                    });
                }

                // Return a success response with the user data
                return Ok(new
                {
                    Success = true,
                    Message = "Active users retrieved successfully.",
                    Data = users
                });
            }
            catch (Exception ex)
            {
                // Log the exception for debugging
                AppLogic.LogException(nameof(GetAllActiveUsers), ex.Message, ex.StackTrace);

                // Return an error response
                return StatusCode(500, new
                {
                    Success = false,
                    Message = "An error occurred while fetching active users.",
                    Details = ex.Message,
                    Data = new List<UserModel>()
                });
            }
        }
        //
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginDto loginDto)
        {
            // Basic validation for empty fields
            if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
            {
                return BadRequest(new { Status = "Error", Message = "Email or password cannot be empty." });
            }

            var result = _userRepository.AuthenticateUser(loginDto.Email, loginDto.Password);

            switch (result)
            {
                case "User does not exist":
                    return NotFound(new { Status = "Error", Message = "User does not exist." });

                case "Wrong password":
                    return BadRequest(new { Status = "Error", Message = "Wrong password." });

                case "Success":
                    return Ok(new { Status = "Success", Message = "Login successful." });

                default:
                    // "Failure" or any unexpected string
                    return StatusCode(500, new { Status = "Error", Message = "An error occurred during login." });
            }
        }
    }
}
