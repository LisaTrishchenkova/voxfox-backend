namespace VoxFox.Interfaces.Question;

public interface IQuestionRepository
{    Task<Models.Entities.Question?> GetByIdAsync(Guid id);
	Task<IList<Models.Entities.Question>> GetByLessonIdAsync(Guid lessonId);
	Task<Models.Entities.Question> AddAsync(Models.Entities.Question question);
	Task<Models.Entities.Question> UpdateAsync(Models.Entities.Question question);
	Task<bool> DeleteAsync(Models.Entities.Question question);

}
