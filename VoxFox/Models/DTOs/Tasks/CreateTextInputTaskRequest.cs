using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.DTOs.Tasks;

public class CreateTextInputTaskRequest
{
	[Required]
	[MinLength(5)]
	public string Question { get; set; } = null!;

	[Required]
	public string CorrectAnswer { get; set; } = null!;

	public string? Explanation { get; set; }
	public List<string>? Hints { get; set; }

	[Range(1, 100)]
	public int Points { get; set; } = 1;

	public bool IsRequired { get; set; } = true;
}
