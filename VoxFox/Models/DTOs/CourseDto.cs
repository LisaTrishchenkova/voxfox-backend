using VoxFox.Enums;

namespace VoxFox.Models.DTOs;

public class CourseDto
{
    public Guid Id { get; init; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public string? FullDescription { get; set; } = null!;
    public CourseStatus Status { get; set; }
    public CourseLevel Level { get; set; }
    public string? CoverImageUrl { get; init; }
    public decimal Price { get; set; }
    public bool IsFree => Price == 0;
    public bool CertificateEnabled { get; set; }
    public int EnrollmentCount { get; set; }
    public decimal Rating { get; set; }
    public int DurationMinutes { get; set; }
    public Guid? ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewStartedAt { get; set; }
    public int ReviewCount { get; set; }

    public Guid? CategoryId { get; set; }
    public DateTime? PublishedAt { get; set; }
    public DateTime CreatedAt { get; init; }
    public AuthorDto? Author { get; set; }
    public ICollection<TagDto>? Tags { get; set; } = new List<TagDto>();
    public bool IsDeleted { get; set; }
}
