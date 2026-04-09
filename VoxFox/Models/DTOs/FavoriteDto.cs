namespace VoxFox.Models.DTOs;

public class FavoriteDto
{
	public Guid Id { get; set; }
	public Guid CourseId { get; set; }
	public DateTime CreatedAt { get; set; }
	public CourseDto Course { get; set; } = null!;
}
