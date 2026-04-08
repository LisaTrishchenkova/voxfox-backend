namespace VoxFox.Models.DTOs.Tasks;

public class SubmitTaskRequest
{
	public int? AnswerIndex { get; set; }
	public List<int>? AnswerIndexes { get; set; }
	public string? AnswerText { get; set; }
}
