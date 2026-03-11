using System.ComponentModel.DataAnnotations;
namespace VoxFox.Models.DTOs
{
    public class CreateAuthorDto
    {
        [Required]
        [MinLength(2)]
        public string Name { get; set; } = null!;
    }
}
