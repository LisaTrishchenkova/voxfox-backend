using MediatR;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Features.Tasks.Queries.GetMySubmission;

public class GetMySubmissionQuery : IRequest<TaskSubmissionDto?>
{
	public Guid TaskId { get; set; }
	public Guid UserId { get; set; }
}
