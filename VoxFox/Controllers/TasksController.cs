using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Features.Tasks.Commands.CreateMultiChoiceTask;
using VoxFox.Features.Tasks.Commands.CreateSingleChoiceTask;
using VoxFox.Features.Tasks.Commands.CreateTextInputTask;
using VoxFox.Features.Tasks.Commands.DeleteTask;
using VoxFox.Features.Tasks.Commands.ReorderTasks;
using VoxFox.Features.Tasks.Commands.SubmitTask;
using VoxFox.Features.Tasks.Commands.UpdateTask;
using VoxFox.Features.Tasks.Queries.GetMySubmission;
using VoxFox.Features.Tasks.Queries.GetSubmissions;
using VoxFox.Features.Tasks.Queries.GetTaskById;
using VoxFox.Features.Tasks.Queries.GetTasksByLesson;
using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Controllers;

public class TasksController : ControllerBase
{
	private readonly IMediator _mediator;
	private readonly ILogger<TasksController> _logger;


	public TasksController(IMediator mediator, ILogger<TasksController> logger)
	{
		_mediator = mediator;
		_logger = logger;
	}

	[HttpPost("api/lessons/{lessonId}/tasks/single-choice")]
	[Authorize(Roles = "Teacher,Admin")]
	[ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskTeacherDto))]
	[ProducesResponseType(StatusCodes.Status400BadRequest)]
	public async Task<ActionResult<TaskTeacherDto>> CreateSingleChoiceTask(
		[FromRoute] Guid lessonId,
		[FromBody] CreateSingleChoiceTaskRequest request)
	{
		try
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
		catch (System.Exception ex)
		{
			_logger.LogError(ex, "Ошибка при создании задания с одним выбором для урока {LessonId}", lessonId);
			return BadRequest(new { error = ex.Message });
		}
	}

	[HttpPost("api/lessons/{lessonId}/tasks/multi-choice")]
	[Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskTeacherDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskTeacherDto>> CreateMultiChoiceTask(
        [FromRoute] Guid lessonId,
        [FromBody] CreateMultiChoiceTaskRequest request)
    {
	    try
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
	    catch (System.Exception ex)
	    {
		    _logger.LogError(ex, "Ошибка при создании задания с множественным выбором для урока {LessonId}", lessonId);
		    return BadRequest(new { error = ex.Message });
	    }
    }

    [HttpPost("api/lessons/{lessonId}/tasks/text-input")]
    [Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TaskTeacherDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskTeacherDto>> CreateTextInputTask(
        [FromRoute] Guid lessonId,
        [FromBody] CreateTextInputTaskRequest request)
    {
	    try
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
	    catch (System.Exception ex)
	    {
		    _logger.LogError(ex, "Ошибка при создании текстового задания для урока {LessonId}", lessonId);
		    return BadRequest(new { error = ex.Message });
	    }
    }

    [HttpGet("api/lessons/{lessonId}/tasks")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IList<object>>> GetTasksByLesson(
	    [FromRoute] Guid lessonId)
    {

	    try
	    {
		    var query = new GetTasksByLessonQuery
		    {
			    LessonId = lessonId,
			    IsTeacher = User.IsInRole("Teacher") || User.IsInRole("Admin")
		    };

		    var result = await _mediator.Send(query);
		    return Ok(result);
	    }
	    catch (System.Exception ex)
	    {
		    _logger.LogError(ex, "Ошибка при получении заданий урока {LessonId}", lessonId);
		    return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
	    }
    }

    [HttpGet("api/tasks/{id}")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetTaskById(
        [FromRoute] Guid id,
        [FromQuery] bool isTeacher = false)
    {
	    try
	    {
		    var query = new GetTaskByIdQuery
		    {
			    TaskId = id,
			    IsTeacher = User.IsInRole("Teacher") || User.IsInRole("Admin")
		    };

		    var result = await _mediator.Send(query);
		    if (result == null)
			    return NotFound(new { error = $"Задание с id {id} не найдено" });

		    return Ok(result);
	    }
	    catch (System.Exception ex)
	    {
		    _logger.LogError(ex, "Ошибка при получении задания {TaskId}", id);
		    return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
	    }
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
	    try
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

		    return CreatedAtAction(nameof(GetMySubmission), new { id }, result);
	    }
	    catch (System.Exception ex)
	    {
		    _logger.LogError(ex, "Ошибка при отправке ответа на задание {TaskId} пользователем {UserId}", id, User.GetUserId());
		    return BadRequest(new { error = ex.Message });
	    }
    }

    [HttpGet("api/tasks/{id}/submissions")]
    [Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<TaskSubmissionDto>))]
    public async Task<ActionResult<IList<TaskSubmissionDto>>> GetSubmissions(
        [FromRoute] Guid id)
    {
	    try
	    {
		    var query = new GetSubmissionsQuery { TaskId = id };
		    var result = await _mediator.Send(query);
		    return Ok(result);
	    }
	    catch (System.Exception ex)
	    {
		    _logger.LogError(ex, "Ошибка при получении ответов на задание {TaskId}", id);
		    return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
	    }
    }

    [HttpPut("api/tasks/reorder")]
    [Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ReorderTasks(
        [FromBody] ReorderTasksRequest request)
    {
	    try
	    {
		    var command = new ReorderTasksCommand
		    {
			    LessonId = request.LessonId,
			    TaskIds = request.TaskIds
		    };

		    await _mediator.Send(command);
		    return NoContent();
	    }
	    catch (System.Exception ex)
	    {
		    _logger.LogError(ex, "Ошибка при изменении порядка заданий в уроке {LessonId}", request.LessonId);
		    return BadRequest(new { error = ex.Message });
	    }
    }

    [HttpPut("api/tasks/{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskTeacherDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TaskTeacherDto>> UpdateTask(
	    [FromRoute] Guid id,
	    [FromBody] UpdateTaskRequest request)
    {
	    try
	    {
		    var command = new UpdateTaskCommand
		    {
			    TaskId = id,
			    Question = request.Question,
			    Options = request.Options,
			    CorrectIndex = request.CorrectIndex,
			    CorrectIndexes = request.CorrectIndexes,
			    CorrectAnswer = request.CorrectAnswer,
			    Explanation = request.Explanation,
			    Hints = request.Hints,
			    Points = request.Points,
			    IsRequired = request.IsRequired
		    };

		    var result = await _mediator.Send(command);
		    if (result == null)
			    return NotFound(new { error = $"Задание с id {id} не найдено" });

		    return Ok(result);
	    }
	    catch (System.Exception ex)
	    {
		    _logger.LogError(ex, "Ошибка при обновлении задания {TaskId}", id);
		    return BadRequest(new { error = ex.Message });
	    }
    }

    [HttpDelete("api/tasks/{id}")]
    [Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask([FromRoute] Guid id)
    {
	    try
	    {
		    await _mediator.Send(new DeleteTaskCommand { TaskId = id });
		    return NoContent();
	    }
	    catch (System.Exception ex)
	    {
		    _logger.LogError(ex, "Ошибка при удалении задания {TaskId}", id);
		    return BadRequest(new { error = ex.Message });
	    }
    }

    [HttpGet("api/tasks/{id}/my-submission")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TaskSubmissionDto))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TaskSubmissionDto?>> GetMySubmission([FromRoute] Guid id)
    {
	    try
	    {
		    var userId = User.GetUserId();
		    if (userId == null)
			    return Unauthorized(new { error = "Не удалось определить пользователя из токена" });

		    var query = new GetMySubmissionQuery
		    {
			    TaskId = id,
			    UserId = userId.Value
		    };

		    var result = await _mediator.Send(query);
		    if (result == null)
			    return NotFound(new { error = "Ответ не найден" });

		    return Ok(result);
	    }
	    catch (System.Exception ex)
	    {
		    _logger.LogError(ex, "Ошибка при получении своего ответа на задание {TaskId}", id);
		    return StatusCode(500, new { error = "Внутренняя ошибка сервера" });
	    }
    }
}
