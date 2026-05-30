using VoxFox.Models.DTOs;

namespace VoxFox.Interfaces.Review;

public interface IReviewService
{
	Task<ServiceResult<ReviewDto>> CreateReviewAsync(Guid courseId, Guid userId, CreateReviewDto dto);
	Task<ServiceResult<IList<ReviewDto>>> GetCourseReviewsAsync(Guid courseId);
	Task<ServiceResult<ReviewDto>> UpdateReviewAsync(Guid reviewId, Guid userId, UpdateReviewDto dto);
	Task<ServiceResult<bool>> DeleteReviewAsync(Guid reviewId, Guid userId, string userRole);
}
