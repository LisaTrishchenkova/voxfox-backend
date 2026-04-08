using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.DTOs.Tasks;

public class ReorderTasksRequest
{
	[Required]
	public Guid LessonId { get; set; }

	[Required]
	public List<Guid> TaskIds { get; set; } = new();
}
