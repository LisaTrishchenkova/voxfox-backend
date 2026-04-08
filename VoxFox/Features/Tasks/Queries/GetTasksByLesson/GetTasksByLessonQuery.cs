using MediatR;

namespace VoxFox.Features.Tasks.Queries.GetTasksByLesson;

public class GetTasksByLessonQuery: IRequest<IList<object>>
{
	public Guid LessonId { get; init; }
	public bool IsTeacher { get; init; }
}
