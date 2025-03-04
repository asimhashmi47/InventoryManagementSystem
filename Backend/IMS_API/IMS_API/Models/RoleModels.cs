namespace IMS_API.Models
{
    // Represents the core Role entity
    public class RoleModel
    {
        public int RoleID { get; set; }
        public string RoleName { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime UpdatedOn { get; set; }
    }

    // DTO for creating or updating a role
    public class CreateOrUpdateRoleDto
    {
        public string RoleName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
