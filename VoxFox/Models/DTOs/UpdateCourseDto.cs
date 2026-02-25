using System.ComponentModel.DataAnnotations;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;
public class UpdateCourseDto
{
    [MinLength(2)]
    public string? Title { get; set; }

    [MinLength(10)]
    public string? Description { get; set; }

     public ICollection<TagDto> Tags { get; set; } = new List<TagDto>();
}
