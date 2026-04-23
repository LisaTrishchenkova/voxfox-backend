namespace VoxFox.Interfaces.Review;

public interface IReviewRepository
{
	Task<Models.Entities.Review?> GetByIdAsync(Guid id);
	Task<Models.Entities.Review?> GetByUserAndCourseAsync(Guid userId, Guid courseId);
	Task<IList<Models.Entities.Review>> GetByCourseIdAsync(Guid courseId);
	Task<Models.Entities.Review> AddAsync(Models.Entities.Review review);
	Task<Models.Entities.Review> UpdateAsync(Models.Entities.Review review);
	Task<bool> DeleteAsync(Models.Entities.Review review);
	Task<double> GetAverageRatingAsync(Guid courseId);
}
