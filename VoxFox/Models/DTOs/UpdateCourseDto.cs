using System.ComponentModel.DataAnnotations;
public class UpdateCourseDto
{
    [MinLength(2)]
    public string? Title { get; set; }

    [MinLength(10)]
    public string? Description { get; set; }

    [MinLength(8)]
    public string? Tags { get; set; }
}
