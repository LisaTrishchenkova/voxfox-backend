using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VoxFox.Extensions;
using VoxFox.Interfaces.Question;
using VoxFox.Models.DTOs.Question;

namespace VoxFox.Controllers;

[ApiController]
[Authorize]
public class QuestionsController : ControllerBase
{
    private readonly IQuestionService _questionService;

    public QuestionsController(IQuestionService questionService)
    {
        _questionService = questionService;
    }

    [HttpPost("api/Lessons/{lessonId}/questions")]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(QuestionDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionDto>> CreateQuestion(
        [FromRoute] Guid lessonId,
        [FromBody] CreateQuestionDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _questionService.CreateQuestionAsync(lessonId, userId.Value, dto);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return CreatedAtAction(nameof(GetLessonQuestions), new { lessonId }, result.Data);
    }

    [HttpGet("api/Lessons/{lessonId}/questions")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<QuestionDto>))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IList<QuestionDto>>> GetLessonQuestions(
        [FromRoute] Guid lessonId)
    {
        var result = await _questionService.GetLessonQuestionsAsync(lessonId);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpPost("api/questions/{questionId}/answer")]
    [Authorize(Roles = "Teacher,Admin")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(QuestionDto))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<QuestionDto>> AnswerQuestion(
        [FromRoute] Guid questionId,
        [FromBody] AnswerQuestionDto dto)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var result = await _questionService.AnswerQuestionAsync(questionId, userId.Value, dto);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return Ok(result.Data);
    }

    [HttpDelete("api/questions/{questionId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteQuestion([FromRoute] Guid questionId)
    {
        var userId = User.GetUserId();
        if (userId == null) return Unauthorized();

        var role = User.GetUserRole()?.ToString() ?? "Student";

        var result = await _questionService.DeleteQuestionAsync(questionId, userId.Value, role);
        if (!result.Success)
            return StatusCode(result.StatusCode ?? 400, new { error = result.Message });

        return NoContent();
    }
}
