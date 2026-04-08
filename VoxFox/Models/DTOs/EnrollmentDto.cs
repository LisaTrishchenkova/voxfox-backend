using VoxFox.Enums;

namespace VoxFox.Models.DTOs;

public class EnrollmentDto
{
	public Guid Id { get; init; }
	public Guid UserId { get; init; }
	public Guid CourseId { get; init; }
	public EnrollmentStatus Status { get; init; }
	public int ProgressPercent { get; init; }
	public DateTime EnrolledAt { get; init; }
	public DateTime? CompletedAt { get; init; }
	public CourseDto? Course { get; init; }
}
