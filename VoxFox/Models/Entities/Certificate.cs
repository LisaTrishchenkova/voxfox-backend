namespace VoxFox.Models.Entities;

public class Certificate
{
	public Guid Id { get; set; }
	public Guid UserId { get; set; }
	public Guid CourseId { get; set; }
	public Guid EnrollmentId { get; set; }
	public string VerificationToken { get; set; } = null!;
	public DateTime IssuedAt { get; set; }

	public User? User { get; set; }
	public Course? Course { get; set; }
	public Enrollment? Enrollment { get; set; }
}
