using VoxFox.Enums;

namespace VoxFox.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Password { get; set; } = default!;
        public string Email { get; set; } = default!;
        public UserRole Role { get; set; } = UserRole.Student;
        public string? AvatarUrl { get; set; }
        public string? Bio { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsDeleted { get; set; } = false;

        public ICollection<Course> Courses { get; set; } = [];
    }
}
