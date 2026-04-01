using System.ComponentModel.DataAnnotations;
using VoxFox.Enums;

namespace VoxFox.Models.DTOs;

public class UpdateCourseDto
{
    [MinLength(2)]
    [MaxLength(200)]
    public string? Title { get; set; }

    [MinLength(10)]
    [MaxLength(500)]
    public string? Description { get; set; }

    public string? FullDescription { get; set; }

    public Guid? CategoryId { get; set; }

    public string? CoverImageUrl { get; set; }

    [Range(0, 1_000_000)]
    public decimal? Price { get; set; }

    public CourseLevel? Level { get; set; }

    public bool? CertificateEnabled { get; set; }

    public ICollection<TagDto> Tags { get; set; } = new List<TagDto>();
}
