using System.ComponentModel.DataAnnotations;
using VoxFox.Enums;

namespace VoxFox.Models.DTOs;

public class CreateCourseDto
{
    [Required]
    [MinLength(2)]
    [MaxLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    [MinLength(10)]
    [MaxLength(500)]
    public string Description { get; set; } = null!;

    public string? FullDescription { get; set; }
    public Guid? CategoryId { get; set; }
    public string? CoverImageUrl { get; set; }
    [Range(0, 1000000)] public decimal Price { get; set; } = 0;

    public CourseLevel Level { get; set; } = CourseLevel.Beginner;
    public bool CertificateEnabled { get; set; } = false;
    public ICollection<TagDto> Tags { get; set; } = new List<TagDto>();

}
