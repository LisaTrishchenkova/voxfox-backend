using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces.Admin;
using VoxFox.Models.DTOs.Admin;
using VoxFox.Models.Entities;

namespace VoxFox.Services;

public class AdminService : IAdminService
{
	 private readonly ApplicationContext _context;

    public AdminService(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<AdminStatsDto> GetStatsAsync()
    {
        var now = DateTime.UtcNow;
        var monthStart = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalUsers = await _context.Users.IgnoreQueryFilters()
            .CountAsync(u => !u.IsDeleted);

        var newUsersThisMonth = await _context.Users.IgnoreQueryFilters()
            .CountAsync(u => !u.IsDeleted && u.CreatedAt >= monthStart);

        var blockedUsers = await _context.Users.IgnoreQueryFilters()
            .CountAsync(u => u.IsBlocked && !u.IsDeleted);

        var totalCourses = await _context.Courses.IgnoreQueryFilters()
            .CountAsync(c => !c.IsDeleted);

        var publishedCourses = await _context.Courses.IgnoreQueryFilters()
            .CountAsync(c => !c.IsDeleted && c.Status == CourseStatus.Published);

        var pendingCourses = await _context.Courses.IgnoreQueryFilters()
            .CountAsync(c => !c.IsDeleted && c.Status == CourseStatus.UnderReview);

        var draftCourses = await _context.Courses.IgnoreQueryFilters()
            .CountAsync(c => !c.IsDeleted && c.Status == CourseStatus.Draft);

        var totalEnrollments = await _context.Enrollments.CountAsync();

        var completedEnrollments = await _context.Enrollments
            .CountAsync(e => e.Status == EnrollmentStatus.Completed);

        var totalCertificates = await _context.Certificates.CountAsync();

        var activeTeachers = await _context.Users.IgnoreQueryFilters()
            .CountAsync(u => u.Role == UserRole.Teacher && !u.IsDeleted && u.Courses.Any());

        var topCourses = await _context.Courses.IgnoreQueryFilters()
            .Where(c => !c.IsDeleted && c.Status == CourseStatus.Published)
            .OrderByDescending(c => c.EnrollmentCount)
            .Take(5)
            .Select(c => new TopCourseDto
            {
                Id = c.Id,
                Title = c.Title,
                AuthorName = c.Author != null ? c.Author.Name : "—",
                EnrollmentCount = c.EnrollmentCount,
                Rating = c.Rating,
            })
            .ToListAsync();

        return new AdminStatsDto
        {
            TotalUsers = totalUsers,
            NewUsersThisMonth = newUsersThisMonth,
            BlockedUsers = blockedUsers,
            TotalCourses = totalCourses,
            PublishedCourses = publishedCourses,
            PendingCourses = pendingCourses,
            DraftCourses = draftCourses,
            TotalEnrollments = totalEnrollments,
            CompletedEnrollments = completedEnrollments,
            TotalCertificates = totalCertificates,
            ActiveTeachers = activeTeachers,
            TopCoursesByEnrollments = topCourses,
        };
    }

    public async Task<IList<ModeratorStatsDto>> GetModeratorsStatsAsync()
    {
        var moderators = await _context.Users.IgnoreQueryFilters()
            .Where(u => u.Role == UserRole.Moderator && !u.IsDeleted)
            .ToListAsync();

        var result = new List<ModeratorStatsDto>();

        foreach (var mod in moderators)
        {
            var currentlyReviewing = await _context.Courses.IgnoreQueryFilters()
                .CountAsync(c => c.ReviewerId == mod.Id && c.Status == CourseStatus.UnderReview);

            result.Add(new ModeratorStatsDto
            {
                ModeratorId = mod.Id,
                ModeratorName = mod.Name,
                CurrentlyReviewing = currentlyReviewing,
                // TotalReviewed/Approved/Rejected требуют отдельной таблицы истории
                // оставляем 0 пока нет ReviewHistory
                TotalReviewed = 0,
                TotalApproved = 0,
                TotalRejected = 0,
            });
        }

        return result;
    }

    public async Task<ServiceResult<bool>> BlockUserAsync(Guid userId, string? reason)
    {
        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return ServiceResult<bool>.Fail("Пользователь не найден", 404);

        if (user.IsBlocked)
            return ServiceResult<bool>.Fail("Пользователь уже заблокирован");

        user.IsBlocked = true;
        user.BlockedAt = DateTime.UtcNow;
        user.BlockReason = reason;
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> UnblockUserAsync(Guid userId)
    {
        var user = await _context.Users.IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return ServiceResult<bool>.Fail("Пользователь не найден", 404);

        if (!user.IsBlocked)
            return ServiceResult<bool>.Fail("Пользователь не заблокирован");

        user.IsBlocked = false;
        user.BlockedAt = null;
        user.BlockReason = null;
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> UnpublishCourseAsync(Guid courseId)
    {
        var course = await _context.Courses.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return ServiceResult<bool>.Fail("Курс не найден", 404);

        if (course.Status != CourseStatus.Published)
            return ServiceResult<bool>.Fail("Курс не опубликован");

        course.Status = CourseStatus.Draft;
        course.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<bool>> ForceReleaseCourseAsync(Guid courseId)
    {
        var course = await _context.Courses.IgnoreQueryFilters()
            .FirstOrDefaultAsync(c => c.Id == courseId);

        if (course == null)
            return ServiceResult<bool>.Fail("Курс не найден", 404);

        if (course.ReviewerId == null)
            return ServiceResult<bool>.Fail("Курс не захвачен ни одним модератором");

        course.ReviewerId = null;
        course.ReviewStartedAt = null;
        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Ok(true);
    }
}
