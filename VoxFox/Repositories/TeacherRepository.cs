using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces.Teacher;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories;

public class TeacherRepository : ITeacherRepository
{
private readonly ApplicationContext _context;
        private readonly ILogger<TeacherRepository> _logger;

        public TeacherRepository(ApplicationContext context, ILogger<TeacherRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<List<TeacherCourseStatsDto>> GetCourseStatsAsync(Guid teacherId)
        {
            try
            {
                // Берём все курсы преподавателя (включая черновики, исключая удалённые)
                var courses = await _context.Courses
                    .IgnoreQueryFilters()
                    .Where(c => c.AuthorId == teacherId && !c.IsDeleted)
                    .Select(c => new
                    {
                        c.Id,
                        c.Title,
                        c.CoverImageUrl,
                        c.Status,
                        c.Price,
                        c.Rating,
                        c.PublishedAt,
                        c.CreatedAt,
                    })
                    .ToListAsync();

                if (!courses.Any())
                    return new List<TeacherCourseStatsDto>();

                var courseIds = courses.Select(c => c.Id).ToList();

                // Enrollment статистика по каждому курсу одним запросом
                var enrollmentStats = await _context.Enrollments
                    .Where(e => courseIds.Contains(e.CourseId))
                    .GroupBy(e => new { e.CourseId, e.Status })
                    .Select(g => new
                    {
                        g.Key.CourseId,
                        g.Key.Status,
                        Count = g.Count(),
                        AvgProgress = g.Average(e => (double)e.ProgressPercent),
                    })
                    .ToListAsync();

                // Число отзывов по каждому курсу
                var reviewStats = await _context.Reviews
                    .Where(r => courseIds.Contains(r.CourseId))
                    .GroupBy(r => r.CourseId)
                    .Select(g => new { CourseId = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Сертификаты по каждому курсу
                var certStats = await _context.Certificates
                    .Where(c => courseIds.Contains(c.CourseId))
                    .GroupBy(c => c.CourseId)
                    .Select(g => new { CourseId = g.Key, Count = g.Count() })
                    .ToListAsync();

                // Заработок по каждому курсу (Earning-транзакции преподавателя)
                var earningsStats = await _context.Transactions
                    .Where(t => t.UserId == teacherId
                             && t.Type == TransactionType.Earning
                             && t.CourseId != null
                             && courseIds.Contains(t.CourseId!.Value))
                    .GroupBy(t => t.CourseId!.Value)
                    .Select(g => new { CourseId = g.Key, Total = g.Sum(t => t.Amount) })
                    .ToListAsync();

                // Собираем итоговые DTO
                return courses.Select(c =>
                {
                    var active = enrollmentStats
                        .FirstOrDefault(e => e.CourseId == c.Id && e.Status == EnrollmentStatus.Active);
                    var completed = enrollmentStats
                        .FirstOrDefault(e => e.CourseId == c.Id && e.Status == EnrollmentStatus.Completed);
                    var allEnrollments = enrollmentStats.Where(e => e.CourseId == c.Id).ToList();

                    var activeCount = active?.Count ?? 0;
                    var completedCount = completed?.Count ?? 0;
                    var totalCount = allEnrollments.Sum(e => e.Count);

                    // Средний прогресс — взвешенное среднее по всем enrollment
                    var avgProgress = totalCount > 0
                        ? allEnrollments.Sum(e => e.AvgProgress * e.Count) / totalCount
                        : 0;

                    return new TeacherCourseStatsDto
                    {
                        CourseId = c.Id,
                        Title = c.Title,
                        CoverImageUrl = c.CoverImageUrl,
                        Status = c.Status.ToString(),
                        Price = c.Price,
                        ActiveStudents = activeCount,
                        CompletedStudents = completedCount,
                        TotalStudents = totalCount,
                        AverageProgress = (decimal)Math.Round(avgProgress, 1),
                        Rating = c.Rating,
                        ReviewCount = reviewStats.FirstOrDefault(r => r.CourseId == c.Id)?.Count ?? 0,
                        Earnings = earningsStats.FirstOrDefault(e => e.CourseId == c.Id)?.Total ?? 0,
                        CertificatesIssued = certStats.FirstOrDefault(cert => cert.CourseId == c.Id)?.Count ?? 0,
                        PublishedAt = c.PublishedAt,
                        CreatedAt = c.CreatedAt,
                    };
                }).OrderByDescending(c => c.TotalStudents).ToList();
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики курсов преподавателя {TeacherId}", teacherId);
                throw;
            }
        }

        public async Task<decimal> GetTotalEarningsAsync(Guid teacherId)
        {
            // Сумма всех Earning-транзакций преподавателя
            return await _context.Transactions
                .Where(t => t.UserId == teacherId && t.Type == TransactionType.Earning)
                .SumAsync(t => t.Amount);
        }

        public async Task<decimal> GetEarningsThisMonthAsync(Guid teacherId)
        {
            var startOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1, 0, 0, 0, DateTimeKind.Utc);

            return await _context.Transactions
                .Where(t => t.UserId == teacherId
                         && t.Type == TransactionType.Earning
                         && t.CreatedAt >= startOfMonth)
                .SumAsync(t => t.Amount);
        }

        public async Task<int> GetTotalCertificatesAsync(Guid teacherId)
        {
            // Сертификаты по всем курсам преподавателя
            return await _context.Certificates
                .Where(c => c.Course != null && c.Course.AuthorId == teacherId)
                .CountAsync();
        }

        public async Task<int> GetCompletedEnrollmentsAsync(Guid teacherId)
        {
            return await _context.Enrollments
                .Where(e => e.Course != null
                         && e.Course.AuthorId == teacherId
                         && e.Status == EnrollmentStatus.Completed)
                .CountAsync();
        }

        public async Task<decimal> GetCourseEarningsAsync(Guid teacherId, Guid courseId)
        {
            return await _context.Transactions
                .Where(t => t.UserId == teacherId
                         && t.Type == TransactionType.Earning
                         && t.CourseId == courseId)
                .SumAsync(t => t.Amount);
        }
}
