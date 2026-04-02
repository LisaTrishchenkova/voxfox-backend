using MediatR;
using VoxFox.Enums;
using VoxFox.Exception;
using VoxFox.Interfaces.Enrollment;
using VoxFox.Interfaces.Task;
using VoxFox.Models.DTOs.Tasks;
using ValidationException = System.ComponentModel.DataAnnotations.ValidationException;

namespace VoxFox.Features.Tasks.Commands.SubmitTask;

public class SubmitTaskHandler :  IRequestHandler<SubmitTaskCommand, TaskSubmissionDto>
{
 private readonly ITaskRepository _taskRepository;
    private readonly IEnrollmentRepository _enrollmentRepository;

    public SubmitTaskHandler(
        ITaskRepository taskRepository,
        IEnrollmentRepository enrollmentRepository)
    {
        _taskRepository = taskRepository;
        _enrollmentRepository = enrollmentRepository;
    }

    public async Task<TaskSubmissionDto> Handle(
        SubmitTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await _taskRepository.GetByIdWithLessonAsync(request.TaskId);
        if (task == null)
            throw new NotFoundException($"Задание с id: {request.TaskId} не найдено");

        if (task.Lesson == null)
            throw new NotFoundException("Урок не найден");

       // var isEnrolled = await _enrollmentRepository
       //     .ExistsAsync(request.UserId, task.Lesson.CourseId);
       // if (!isEnrolled)
         //   throw new ForbiddenException("Вы не записаны на этот курс");

        ValidateSubmit(task.Type, request);

        var lastSubmission = await _taskRepository
            .GetLastSubmissionAsync(request.TaskId, request.UserId);
        var attemptNumber = (lastSubmission?.AttemptNumber ?? 0) + 1;

        var isCorrect = CheckAnswer(task, request);

        var submission = new TaskSubmission
        {
            TaskId = request.TaskId,
            UserId = request.UserId,
            AnswerIndex = request.AnswerIndex,
            AnswerIndexes = request.AnswerIndexes,
            AnswerText = request.AnswerText,
            IsCorrect = isCorrect,
            Score = isCorrect ? task.Points : 0,
            AttemptNumber = attemptNumber,
            SubmittedAt = DateTime.UtcNow
        };

        var created = await _taskRepository.AddSubmissionAsync(submission);
        return MapToDto(created);
    }

    private static void ValidateSubmit(TaskType type, SubmitTaskCommand request)
    {
        var error = type switch
        {
            TaskType.SingleChoice when request.AnswerIndex == null =>
                "SingleChoice требует поле AnswerIndex",
            TaskType.MultiChoice when
                request.AnswerIndexes == null || request.AnswerIndexes.Count == 0 =>
                "MultiChoice требует поле AnswerIndexes",
            TaskType.TextInput when string.IsNullOrWhiteSpace(request.AnswerText) =>
                "TextInput требует поле AnswerText",
            _ => null
        };

        if (error != null)
            throw new ValidationException(error);
    }

    private static bool CheckAnswer(TaskEntity task, SubmitTaskCommand request)
    {
        return task.Type switch
        {
            TaskType.SingleChoice =>
                request.AnswerIndex == task.CorrectIndex,

            TaskType.MultiChoice =>
                request.AnswerIndexes != null &&
                request.AnswerIndexes.OrderBy(x => x)
                    .SequenceEqual((task.CorrectIndexes ?? new()).OrderBy(x => x)),

            // без учёта регистра + проверка что все ключевые слова из CorrectAnswer
            // присутствуют в ответе студента
            TaskType.TextInput =>
                CheckTextAnswer(request.AnswerText, task.CorrectAnswer),

            _ => false
        };
    }

    private static bool CheckTextAnswer(string? studentAnswer, string? correctAnswer)
    {
        if (string.IsNullOrWhiteSpace(studentAnswer) ||
            string.IsNullOrWhiteSpace(correctAnswer))
            return false;

        var studentLower = studentAnswer.ToLower();

        // разбиваем правильный ответ на ключевые слова (слова длиннее 2 символов)
        var keywords = correctAnswer
            .ToLower()
            .Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2)
            .ToList();

        if (keywords.Count == 0)
            return string.Equals(studentAnswer, correctAnswer,
                StringComparison.OrdinalIgnoreCase);

        // все ключевые слова должны присутствовать в ответе студента
        return keywords.All(keyword => studentLower.Contains(keyword));
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
