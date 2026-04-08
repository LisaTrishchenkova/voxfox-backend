using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.DTOs.Tasks;

public class CreateSingleChoiceTaskRequest
{
	[Required]
	[MinLength(5)]
	public string Question { get; set; } = null!;

	[Required]
	public List<string> Options { get; set; } = new();

	[Required]
	public int CorrectIndex { get; set; }

	public string? Explanation { get; set; }
	public List<string>? Hints { get; set; }

	[Range(1, 100)]
	public int Points { get; set; } = 1;

	public bool IsRequired { get; set; } = true;
}
