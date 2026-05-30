using VoxFox.Enums;

namespace VoxFox.Models.Entities;

public class CourseDraft
{
    public Guid Id { get; set; }
    public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;
    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? FullDescription { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal Price { get; set; }
    public CourseLevel Level { get; set; }
    public bool CertificateEnabled { get; set; }
    public Guid? CategoryId { get; set; }

    public DraftStatus Status { get; set; } = DraftStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    public ICollection<DraftSection> Sections { get; set; } = [];
    public ICollection<DraftTag> Tags { get; set; } = [];
}
