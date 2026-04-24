namespace VoxFox.Models.DTOs.Certificate;

public class CertificateDto
{
	public Guid Id { get; set; }
	public Guid CourseId { get; set; }
	public string CourseTitle { get; set; } = null!;
	public string UserName { get; set; } = null!;
	public string VerificationToken { get; set; } = null!;
	public DateTime IssuedAt { get; set; }
}
