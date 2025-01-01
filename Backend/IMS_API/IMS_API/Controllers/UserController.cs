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
        public IActionResult CreateUser(User user)
        {
            _userRepository.CreateUser(user);
            return Ok();
        }

        [HttpGet("{GetUserById}")]
        public ActionResult<User> GetUserById(int id)
        {
            var user = _userRepository.GetUserById(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPut("UpdateUser")]
        public IActionResult UpdateUser(User user)
        {
            _userRepository.UpdateUser(user);
            return Ok();
        }

        [HttpGet("GetAllActiveUsers")]
        public ActionResult<List<User>> GetAllActiveUsers()
        {
            var users = _userRepository.GetAllActiveUsers();
            return Ok(users);
        }
    }
}
