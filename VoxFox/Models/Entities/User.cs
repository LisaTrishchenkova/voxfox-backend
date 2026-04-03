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

        public ICollection<Course> Courses { get; set; } = [];
    }
}
