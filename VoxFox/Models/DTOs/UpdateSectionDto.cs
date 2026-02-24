using System.ComponentModel.DataAnnotations;
namespace VoxFox.Models.DTOs
{
    public class UpdateSectionDto
    {
        [MinLength(2)]
        public string? Title { get; set; } = null!;

        [MinLength(10)]
        public string? Description { get; set; } = null!;
    }
}
