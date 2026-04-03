using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Features.Tasks.Commands.CreateMultiChoiceTask;
using VoxFox.Features.Tasks.Commands.CreateSingleChoiceTask;
using VoxFox.Features.Tasks.Commands.CreateTextInputTask;
using VoxFox.Features.Tasks.Commands.ReorderTasks;
using VoxFox.Features.Tasks.Commands.SubmitTask;
using VoxFox.Features.Tasks.Queries.GetSubmissions;
using VoxFox.Features.Tasks.Queries.GetTaskById;
using VoxFox.Features.Tasks.Queries.GetTasksByLesson;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Controllers;

public class TasksController : ControllerBase
{
	private readonly IMediator _mediator;

	public TasksController(IMediator mediator)
	{
		_mediator = mediator;
	}

	[HttpPost("api/lessons/{lessonId}/tasks/single-choice")]
	[Authorize(Roles = "Teacher,Admin")]
	[ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskTeacherDto))]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<TaskTeacherDto>> CreateSingleChoiceTask(
		[FromRoute] Guid lessonId,
		[FromBody] CreateSingleChoiceTaskRequest request)
	{
		var command = new CreateSingleChoiceTaskCommand
		{
			LessonId = lessonId,
			Question = request.Question,
			Options = request.Options,
			CorrectIndex = request.CorrectIndex,
			Explanation = request.Explanation,
			Hints = request.Hints,
			Points = request.Points,
			IsRequired = request.IsRequired
		};

		var result = await _mediator.Send(command);
		return CreatedAtAction(nameof(GetTaskById), new { id = result.Id }, result);
	}

	[HttpPost("api/lessons/{lessonId}/tasks/multi-choice")]
	[Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskTeacherDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskTeacherDto>> CreateMultiChoiceTask(
        [FromRoute] Guid lessonId,
        [FromBody] CreateMultiChoiceTaskRequest request)
    {
        var command = new CreateMultiChoiceTaskCommand
        {
            LessonId = lessonId,
            Question = request.Question,
            Options = request.Options,
            CorrectIndexes = request.CorrectIndexes,
            Explanation = request.Explanation,
            Hints = request.Hints,
            Points = request.Points,
            IsRequired = request.IsRequired
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTaskById), new { id = result.Id }, result);
    }

    [HttpPost("api/lessons/{lessonId}/tasks/text-input")]
    [Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskTeacherDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskTeacherDto>> CreateTextInputTask(
        [FromRoute] Guid lessonId,
        [FromBody] CreateTextInputTaskRequest request)
    {
        var command = new CreateTextInputTaskCommand
        {
            LessonId = lessonId,
            Question = request.Question,
            CorrectAnswer = request.CorrectAnswer,
            Explanation = request.Explanation,
            Hints = request.Hints,
            Points = request.Points,
            IsRequired = request.IsRequired
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTaskById), new { id = result.Id }, result);
    }

    [HttpGet("api/lessons/{lessonId}/tasks")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<object>>> GetTasksByLesson(
        [FromRoute] Guid lessonId,
        [FromQuery] bool isTeacher = false)
    {
        var query = new GetTasksByLessonQuery
        {
            LessonId = lessonId,
            IsTeacher = isTeacher
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpGet("api/tasks/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetTaskById(
        [FromRoute] Guid id,
        [FromQuery] bool isTeacher = false)
    {
        var query = new GetTaskByIdQuery
        {
            TaskId = id,
            IsTeacher = isTeacher
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost("api/tasks/{id}/submit")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskSubmissionDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TaskSubmissionDto>> SubmitTask(
        [FromRoute] Guid id,
        [FromBody] SubmitTaskRequest request)
    {
        var userId = User.GetUserId();
        if (userId == null)
            return Unauthorized(new { error = "Не удалось определить пользователя из токена" });

        var command = new SubmitTaskCommand
        {
            TaskId = id,
            UserId = userId.Value,
            AnswerIndex = request.AnswerIndex,
            AnswerIndexes = request.AnswerIndexes,
            AnswerText = request.AnswerText
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTaskById), new { id }, result);
    }

    [HttpGet("api/tasks/{id}/submissions")]
    [Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<TaskSubmissionDto>))]
    public async Task<ActionResult<IList<TaskSubmissionDto>>> GetSubmissions(
        [FromRoute] Guid id)
    {
        var query = new GetSubmissionsQuery { TaskId = id };
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPut("api/tasks/reorder")]
    [Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReorderTasks(
        [FromBody] ReorderTasksRequest request)
    {
        var command = new ReorderTasksCommand
        {
            LessonId = request.LessonId,
            TaskIds = request.TaskIds
        };

        await _mediator.Send(command);
        return NoContent();
    }
}
