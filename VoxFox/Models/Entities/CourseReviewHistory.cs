using VoxFox.Enums;

namespace VoxFox.Models.Entities;

public class CourseReviewHistory
{
	public Guid Id { get; set; }
	public Guid CourseId { get; set; }
	public Course Course { get; set; } = null!;
	public Guid ModeratorId { get; set; }
	public User Moderator { get; set; } = null!;
	public ReviewDecision Decision { get; set; }
	public string? Reason { get; set; }
	public DateTime ReviewedAt { get; set; }
}
