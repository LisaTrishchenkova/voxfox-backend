using VoxFox.Enums;

namespace VoxFox.Models.DTOs.Draft.CreateCourseDraftDto;

public class CreateCourseDraftDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? FullDescription { get; set; }
    public string? CoverImageUrl { get; set; }
    public decimal Price { get; set; }
    public CourseLevel Level { get; set; }
    public bool CertificateEnabled { get; set; }
    public Guid? CategoryId { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<CreateDraftSectionDto> Sections { get; set; } = [];

}
