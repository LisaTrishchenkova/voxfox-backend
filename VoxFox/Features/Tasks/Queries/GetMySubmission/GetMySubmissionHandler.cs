using MediatR;
using VoxFox.Interfaces.Task;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Features.Tasks.Queries.GetMySubmission;

public class GetMySubmissionHandler : IRequestHandler<GetMySubmissionQuery, TaskSubmissionDto?>
{
	private readonly ITaskRepository _taskRepository;

	public GetMySubmissionHandler(ITaskRepository taskRepository)
	{
		_taskRepository = taskRepository;
	}
	public async Task<TaskSubmissionDto?> Handle(GetMySubmissionQuery request, CancellationToken cancellationToken)
	{
		var submission = await _taskRepository.GetLastSubmissionAsync(request.TaskId, request.UserId);
		if (submission == null)
			return null;

		return new TaskSubmissionDto
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
}
