using System.ComponentModel.DataAnnotations;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;
public class CreateCourseDto
{
    [Required]
    [MinLength(2)]
    public string Title { get; set; } = null!;

    [Required]
    [MinLength(10)]
    public string Description { get; set; } = null!;
    public Guid? CategoryId { get; set; }

    public Guid? AuthorId {get; set;}
    public ICollection<TagDto> Tags { get; set; } = new List<TagDto>();

}
