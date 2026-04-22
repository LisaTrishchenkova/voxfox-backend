namespace VoxFox.Models.DTOs.Tasks;

public class UpdateTaskRequest
{
	public string? Question { get; set; }
	public List<string>? Options { get; set; }
	public int? CorrectIndex { get; set; }
	public List<int>? CorrectIndexes { get; set; }
	public string? CorrectAnswer { get; set; }
	public string? Explanation { get; set; }
	public List<string>? Hints { get; set; }
	public int? Points { get; set; }
	public bool? IsRequired { get; set; }
}
