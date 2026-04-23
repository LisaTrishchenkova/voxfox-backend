namespace VoxFox.Models.DTOs;

public class ReviewDto
{
	public Guid Id { get; set; }
	public Guid CourseId { get; set; }
	public Guid UserId { get; set; }
	public string? UserName { get; set; }
	public int Rating { get; set; }
	public string? Comment { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? UpdatedAt { get; set; }
}
