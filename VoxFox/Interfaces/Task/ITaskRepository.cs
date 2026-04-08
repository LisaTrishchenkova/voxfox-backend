using VoxFox.Models.DTOs.Tasks;

namespace VoxFox.Interfaces.Task;

public interface ITaskRepository
{
	Task<TaskEntity> AddAsync(TaskEntity task);
	Task<TaskEntity?> GetByIdAsync(Guid id);
	Task<TaskEntity?> GetByIdWithLessonAsync(Guid id);
	Task<IList<TaskEntity>> GetByLessonIdAsync(Guid lessonId);
	Task<int> GetMaxOrderIndexAsync(Guid lessonId);
	Task<bool> DeleteAsync(TaskEntity task);
	Task<TaskSubmission> AddSubmissionAsync(TaskSubmission submission);
	Task<TaskSubmission?> GetLastSubmissionAsync(Guid taskId, Guid userId);
	Task<IList<TaskSubmission>> GetSubmissionsByTaskIdAsync(Guid taskId);
}
