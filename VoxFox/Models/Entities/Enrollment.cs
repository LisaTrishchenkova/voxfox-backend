using VoxFox.Enums;

namespace VoxFox.Models.Entities;

public class Enrollment
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public Guid CourseId { get; set; }
	public EnrollmentStatus Status { get; set; } = EnrollmentStatus.Active;
	public DateTime EnrolledAt { get; set; }
	public DateTime? CompletedAt { get; set; }
	public int ProgressPercent { get; set; } = 0;

	public User? User { get; set; }
	public Course? Course { get; set; }
}
