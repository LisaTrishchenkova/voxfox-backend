namespace VoxFox.Models.Documents;

public class CourseDocument
{
	public Guid Id { get; init; }
	public string Title { get; init; } = string.Empty;
	public string Description { get; init; } = string.Empty;
	public string Tags { get; init; } = string.Empty;
	public Guid? CategoryId { get; init; }
	public decimal Price { get; init; }
	public bool IsPublished { get; init; }
	public DateTime CreatedAt { get; init; }
}
