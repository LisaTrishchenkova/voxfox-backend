namespace VoxFox.Models.Entities;

public class Favorite
{
	 public Guid Id { get; set; }
	 public Guid UserId { get; set; }
	 public User User { get; set; } = null!;
	 public Guid CourseId { get; set; }
	 public Course Course { get; set; } = null!;
	 public DateTime CreatedAt { get; set; }
}
