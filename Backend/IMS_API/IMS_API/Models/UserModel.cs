namespace IMS_API.Models
{
    // This represents the core user entity used in your business logic or database operations.
    public class UserModel
    {
        public int UserID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int RoleID { get; set; }
        public string Role { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO for creating a new user.
    public class CreateUserDto
    {
        public int RoleID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
    }

    // DTO for updating user information.
    public class UpdateUserDto
    {
        public int UserID { get; set; }
        public int RoleID { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public bool IsActive { get; set; }
    }

}
