using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces.Moderation;
using VoxFox.Models.DTOs.Admin;
using VoxFox.Models.DTOs.Moderation;
using VoxFox.Models.Entities;

namespace VoxFox.Services;

public class ModerationService : IModerationService
{
    private readonly ApplicationContext _context;
    private static readonly TimeSpan ClaimTimeout = TimeSpan.FromMinutes(30);

    public ModerationService(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<ServiceResult<bool>> ClaimCourseAsync(Guid courseId, Guid moderatorId)
    {
        var course = await _context.Courses.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return ServiceResult<bool>.Fail("Курс не найден", 404);

        // Принимаем оба статуса модерации
        if (course.Status != CourseStatus.UnderReview && course.Status != CourseStatus.PublishedUnderReview)
            return ServiceResult<bool>.Fail("Курс не находится на модерации");

        // Если уже захвачен этим же модератором — ок
        if (course.ReviewerId == moderatorId)
            return ServiceResult<bool>.Ok(true);

        // Если захвачен другим и ещё не истёк таймаут
        if (course.ReviewerId.HasValue && course.ReviewStartedAt.HasValue)
        {
            var elapsed = DateTime.UtcNow - course.ReviewStartedAt.Value;
            if (elapsed < ClaimTimeout)
                return ServiceResult<bool>.Fail(
                    "Курс уже проверяется другим модератором", 409);
        }

        course.ReviewerId = moderatorId;
        course.ReviewStartedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> ReleaseCourseAsync(Guid courseId, Guid moderatorId)
    {
        var course = await _context.Courses.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return ServiceResult<bool>.Fail("Курс не найден", 404);

        if (course.ReviewerId != moderatorId)
            return ServiceResult<bool>.Fail("Вы не захватывали этот курс", 403);

        course.ReviewerId = null;
        course.ReviewStartedAt = null;
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<CourseReviewDto>> GetCourseForReviewAsync(Guid courseId)
    {
        var course = await _context.Courses.IgnoreQueryFilters()
            .Include(c => c.Author)
            .Include(c => c.Reviewer)
            .Include(c => c.Tags)
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return ServiceResult<CourseReviewDto>.Fail("Курс не найден", 404);

        var dto = new CourseReviewDto
        {
            Id = course.Id,
            Title = course.Title,
            Description = course.Description,
            FullDescription = course.FullDescription,
            CoverImageUrl = course.CoverImageUrl,
            Price = course.Price,
            Level = course.Level.ToString(),
            CertificateEnabled = course.CertificateEnabled,
            ReviewCount = course.ReviewCount,
            CreatedAt = course.CreatedAt,
            SubmittedAt = course.UpdatedAt,
            AuthorId = course.AuthorId,
            AuthorName = course.Author?.Name,
            ReviewerId = course.ReviewerId,
            ReviewerName = course.Reviewer?.Name,
            ReviewStartedAt = course.ReviewStartedAt,
            Tags = course.Tags?.Select(t => t.Name).ToList() ?? new List<string>(),
        };

        return ServiceResult<CourseReviewDto>.Ok(dto);
    }

    public async Task<ModeratorStatsDto> GetMyStatsAsync(Guid moderatorId)
    {
        var moderator = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == moderatorId);

        // Считаем оба статуса модерации
        var currentlyReviewing = await _context.Courses.IgnoreQueryFilters()
            .CountAsync(c => c.ReviewerId == moderatorId
                && (c.Status == CourseStatus.UnderReview || c.Status == CourseStatus.PublishedUnderReview));

        var totalApproved = await _context.CourseReviewHistories
            .CountAsync(h => h.ModeratorId == moderatorId && h.Decision == ReviewDecision.Approved);

        var totalRejected = await _context.CourseReviewHistories
            .CountAsync(h => h.ModeratorId == moderatorId && h.Decision == ReviewDecision.Rejected);

        return new ModeratorStatsDto
        {
            ModeratorId = moderatorId,
            ModeratorName = moderator?.Name ?? "—",
            CurrentlyReviewing = currentlyReviewing,
            TotalReviewed = totalApproved + totalRejected,
            TotalApproved = totalApproved,
            TotalRejected = totalRejected,
        };
    }

    public async Task ReleaseStaleClaimsAsync(TimeSpan timeout)
    {
        var cutoff = DateTime.UtcNow - timeout;

        // Освобождаем зависшие claim для обоих статусов
        var staleCourses = await _context.Courses.IgnoreQueryFilters()
            .Where(c => c.ReviewerId.HasValue
                        && c.ReviewStartedAt.HasValue
                        && c.ReviewStartedAt < cutoff
                        && (c.Status == CourseStatus.UnderReview || c.Status == CourseStatus.PublishedUnderReview))
            .ToListAsync();

        foreach (var course in staleCourses)
        {
            course.ReviewerId = null;
            course.ReviewStartedAt = null;
        }

        if (staleCourses.Count > 0)
            await _context.SaveChangesAsync();
    }
}
