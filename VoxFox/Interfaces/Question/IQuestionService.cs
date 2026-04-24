using VoxFox.Models.DTOs.Question;

namespace VoxFox.Interfaces.Question;

public interface IQuestionService
{
	Task<ServiceResult<QuestionDto>> CreateQuestionAsync(Guid lessonId, Guid userId, CreateQuestionDto dto);
	Task<ServiceResult<IList<QuestionDto>>> GetLessonQuestionsAsync(Guid lessonId);
	Task<ServiceResult<QuestionDto>> AnswerQuestionAsync(Guid questionId, Guid userId, AnswerQuestionDto dto);
	Task<ServiceResult<bool>> DeleteQuestionAsync(Guid questionId, Guid userId, string userRole);
}
