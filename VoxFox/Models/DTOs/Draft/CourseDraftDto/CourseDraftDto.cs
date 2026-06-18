namespace VoxFox.Models.DTOs.Draft.CourseDraftDto;

public class CourseDraftDto
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? FullDescription { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal Price { get; set; }
    public string Level { get; set; } = null!;
    public bool CertificateEnabled { get; set; }
    public Guid? CategoryId { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<DraftSectionDto> Sections { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public string? ReviewerId { get; set; }
    public string? ReviewerName { get; set; }
    public DateTime? ReviewStartedAt { get; set; }
}
