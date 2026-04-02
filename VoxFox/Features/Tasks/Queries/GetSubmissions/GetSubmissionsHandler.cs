using MediatR;
using VoxFox.Interfaces.Task;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Features.Tasks.Queries.GetSubmissions;

public class GetSubmissionsHandler : IRequestHandler<GetSubmissionsQuery, IList<TaskSubmissionDto>>
{
	private readonly ITaskRepository _taskRepository;

	public GetSubmissionsHandler(ITaskRepository taskRepository)
	{
		_taskRepository = taskRepository;
	}

	public async Task<IList<TaskSubmissionDto>> Handle(
		GetSubmissionsQuery request,
		CancellationToken cancellationToken)
	{
		var submissions = await _taskRepository
			.GetSubmissionsByTaskIdAsync(request.TaskId);

		return submissions.Select(MapToDto).ToList();
	}

	private static TaskSubmissionDto MapToDto(TaskSubmission submission) => new()
	{
		Id = submission.Id,
		TaskId = submission.TaskId,
		UserId = submission.UserId,
		AnswerIndex = submission.AnswerIndex,
		AnswerIndexes = submission.AnswerIndexes,
		AnswerText = submission.AnswerText,
		IsCorrect = submission.IsCorrect,
		Score = submission.Score,
		AttemptNumber = submission.AttemptNumber,
		SubmittedAt = submission.SubmittedAt
	};
}
