using System.ComponentModel.DataAnnotations;
public class CreateCourseDto
{
    [Required]
    [MinLength(2)]
    public string Title { get; set; } = null!;

    [Required]
    [MinLength(10)]
    public string Description { get; set; } = null!;

    [MinLength(8)]
    public string Tags { get; set; } = null!;
}
