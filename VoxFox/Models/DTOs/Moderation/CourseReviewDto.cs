namespace VoxFox.Models.DTOs.Moderation;

public class CourseReviewDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = null!;
	public string Description { get; set; } = null!;
	public string? FullDescription { get; set; }
	public string? CoverImageUrl { get; set; }
	public decimal Price { get; set; }
	public string Level { get; set; } = null!;
	public bool CertificateEnabled { get; set; }
	public int ReviewCount { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime? SubmittedAt { get; set; }
	public string? AuthorName { get; set; }
	public Guid? AuthorId { get; set; }
	public string? ReviewerName { get; set; }
	public Guid? ReviewerId { get; set; }
	public DateTime? ReviewStartedAt { get; set; }
	public bool IsClaimed => ReviewerId.HasValue;
	public IList<string> Tags { get; set; } = new List<string>();
}
