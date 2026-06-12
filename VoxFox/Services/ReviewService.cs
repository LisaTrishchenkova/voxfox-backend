using VoxFox.Interfaces;
using VoxFox.Interfaces.Achievement;
using VoxFox.Interfaces.Review;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Services;

public class ReviewService : IReviewService
{
    private readonly IReviewRepository _reviewRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILogger<ReviewService> _logger;
    private readonly IAchievementService _achievementService;

    public ReviewService(
        IReviewRepository reviewRepository,
        ICourseRepository courseRepository,
        ILogger<ReviewService> logger,
        IAchievementService achievementService)
    {
        _reviewRepository = reviewRepository;
        _courseRepository = courseRepository;
        _logger = logger;
        _achievementService = achievementService;
    }

    public async Task<ServiceResult<ReviewDto>> CreateReviewAsync(
        Guid courseId, Guid userId, CreateReviewDto dto)
    {
        if (dto.Rating < 1 || dto.Rating > 5)
            return ServiceResult<ReviewDto>.Fail(
                "Рейтинг должен быть от 1 до 5",
                StatusCodes.Status400BadRequest);

        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            return ServiceResult<ReviewDto>.Fail(
                $"Курс с id {courseId} не найден",
                StatusCodes.Status404NotFound);

        var existing = await _reviewRepository.GetByUserAndCourseAsync(userId, courseId);
        if (existing != null)
            return ServiceResult<ReviewDto>.Fail(
                "Вы уже оставили отзыв на этот курс",
                StatusCodes.Status409Conflict);

        var review = new Review
        {
            CourseId = courseId,
            UserId = userId,
            Rating = dto.Rating,
            Comment = dto.Comment,
            CreatedAt = DateTime.UtcNow
        };

        var created = await _reviewRepository.AddAsync(review);

        await RecalculateCourseRatingAsync(courseId);

        // ─── Ачивки за отзыв ──────────────────────────────────────
        await _achievementService.CheckAndAwardAsync(userId, AchievementTrigger.ReviewCreated);

        return ServiceResult<ReviewDto>.Ok(MapToDto(created));
    }

    public async Task<ServiceResult<IList<ReviewDto>>> GetCourseReviewsAsync(Guid courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            return ServiceResult<IList<ReviewDto>>.Fail(
                $"Курс с id {courseId} не найден",
                StatusCodes.Status404NotFound);

        var reviews = await _reviewRepository.GetByCourseIdAsync(courseId);
        return ServiceResult<IList<ReviewDto>>.Ok(reviews.Select(MapToDto).ToList());
    }

    public async Task<ServiceResult<ReviewDto>> UpdateReviewAsync(
        Guid reviewId, Guid userId, UpdateReviewDto dto)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            return ServiceResult<ReviewDto>.Fail(
                $"Отзыв с id {reviewId} не найден",
                StatusCodes.Status404NotFound);

        if (review.UserId != userId)
            return ServiceResult<ReviewDto>.Fail(
                "Нет доступа — это не ваш отзыв",
                StatusCodes.Status403Forbidden);

        if (dto.Rating.HasValue)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
                return ServiceResult<ReviewDto>.Fail(
                    "Рейтинг должен быть от 1 до 5",
                    StatusCodes.Status400BadRequest);
            review.Rating = dto.Rating.Value;
        }

        if (dto.Comment != null)
            review.Comment = dto.Comment;

        review.UpdatedAt = DateTime.UtcNow;

        var updated = await _reviewRepository.UpdateAsync(review);
        await RecalculateCourseRatingAsync(review.CourseId);

        return ServiceResult<ReviewDto>.Ok(MapToDto(updated));
    }

    public async Task<ServiceResult<bool>> DeleteReviewAsync(
        Guid reviewId, Guid userId, string userRole)
    {
        var review = await _reviewRepository.GetByIdAsync(reviewId);
        if (review == null)
            return ServiceResult<bool>.Fail(
                $"Отзыв с id {reviewId} не найден",
                StatusCodes.Status404NotFound);

        var isAdminOrModerator = userRole is "Admin" or "Moderator";
        if (!isAdminOrModerator && review.UserId != userId)
            return ServiceResult<bool>.Fail(
                "Нет доступа — это не ваш отзыв",
                StatusCodes.Status403Forbidden);

        var courseId = review.CourseId;
        await _reviewRepository.DeleteAsync(review);
        await RecalculateCourseRatingAsync(courseId);

        return ServiceResult<bool>.Ok(true);
    }

    private async Task RecalculateCourseRatingAsync(Guid courseId)
    {
        var avg = await _reviewRepository.GetAverageRatingAsync(courseId);
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null) return;

        course.Rating = (decimal)Math.Round(avg, 2);
        await _courseRepository.UpdateAsync(course);
    }

    private static ReviewDto MapToDto(Review r) => new()
    {
        Id = r.Id,
        CourseId = r.CourseId,
        UserId = r.UserId,
        UserName = r.User?.Name,
        Rating = r.Rating,
        Comment = r.Comment,
        CreatedAt = r.CreatedAt,
        UpdatedAt = r.UpdatedAt
    };
}
