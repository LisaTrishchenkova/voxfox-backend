using VoxFox.Interfaces.Teacher;
using VoxFox.Models.DTOs;

namespace VoxFox.Services;

public class TeacherService  : ITeacherService
{
 private readonly ITeacherRepository _teacherRepository;
        private readonly ILogger<TeacherService> _logger;

        public TeacherService(ITeacherRepository teacherRepository, ILogger<TeacherService> logger)
        {
            _teacherRepository = teacherRepository;
            _logger = logger;
        }

        public async Task<ServiceResult<TeacherStatsDto>> GetStatsAsync(Guid teacherId)
        {
            try
            {
                var courseStats = await _teacherRepository.GetCourseStatsAsync(teacherId);

                var totalStudents = courseStats.Sum(c => c.TotalStudents);
                var publishedCourses = courseStats.Count(c => c.Status == "Published");

                // Взвешенный средний рейтинг — курсы с большим числом отзывов влияют сильнее
                var totalReviews = courseStats.Sum(c => c.ReviewCount);
                var weightedRating = totalReviews > 0
                    ? courseStats
                        .Where(c => c.ReviewCount > 0)
                        .Sum(c => c.Rating * c.ReviewCount) / totalReviews
                    : 0;

                var totalEarnings = await _teacherRepository.GetTotalEarningsAsync(teacherId);
                var earningsThisMonth = await _teacherRepository.GetEarningsThisMonthAsync(teacherId);
                var totalCertificates = await _teacherRepository.GetTotalCertificatesAsync(teacherId);
                var completedEnrollments = await _teacherRepository.GetCompletedEnrollmentsAsync(teacherId);

                return ServiceResult<TeacherStatsDto>.Ok(new TeacherStatsDto
                {
                    TotalStudents = totalStudents,
                    PublishedCourses = publishedCourses,
                    TotalCourses = courseStats.Count,
                    AverageRating = Math.Round(weightedRating, 2),
                    TotalEarnings = totalEarnings,
                    EarningsThisMonth = earningsThisMonth,
                    TotalCertificates = totalCertificates,
                    CompletedEnrollments = completedEnrollments,
                });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики преподавателя {TeacherId}", teacherId);
                return ServiceResult<TeacherStatsDto>.Fail("Ошибка при получении статистики",
                    StatusCodes.Status500InternalServerError);
            }
        }

        public async Task<ServiceResult<List<TeacherCourseStatsDto>>> GetCourseStatsAsync(Guid teacherId)
        {
            try
            {
                var stats = await _teacherRepository.GetCourseStatsAsync(teacherId);
                return ServiceResult<List<TeacherCourseStatsDto>>.Ok(stats);
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Ошибка при получении статистики курсов преподавателя {TeacherId}", teacherId);
                return ServiceResult<List<TeacherCourseStatsDto>>.Fail("Ошибка при получении статистики курсов",
                    StatusCodes.Status500InternalServerError);
            }
        }
}
