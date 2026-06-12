using Microsoft.EntityFrameworkCore;
using VoxFox.Enums;
using VoxFox.Interfaces.Achievement;
using VoxFox.Models;
using VoxFox.Models.Entities;

namespace VoxFox.Services;

public class AchievementService: IAchievementService
    {
        private readonly IAchievementRepository _achievementRepository;
        private readonly ApplicationContext _context;
        private readonly ILogger<AchievementService> _logger;

        public AchievementService(
            IAchievementRepository achievementRepository,
            ApplicationContext context,
            ILogger<AchievementService> logger)
        {
            _achievementRepository = achievementRepository;
            _context = context;
            _logger = logger;
        }

        public async Task<ServiceResult<List<AchievementDto>>> GetUserAchievementsAsync(Guid userId)
        {
            var allAchievements = await _achievementRepository.GetAllAsync();
            var userAchievements = await _achievementRepository.GetUserAchievementsAsync(userId);
            var earnedMap = userAchievements.ToDictionary(ua => ua.Achievement.Code, ua => ua.EarnedAt);

            var result = allAchievements.Select(a => new AchievementDto
            {
                Id = a.Id,
                Code = a.Code,
                Title = a.Title,
                Description = a.Description,
                Icon = a.Icon,
                EarnedAt = earnedMap.TryGetValue(a.Code, out var date) ? date : null,
            }).ToList();

            return ServiceResult<List<AchievementDto>>.Ok(result);
        }

        public async Task<List<NewAchievementDto>> CheckAndAwardAsync(Guid userId, AchievementTrigger trigger)
        {
            var newlyEarned = new List<NewAchievementDto>();

            try
            {
                var codesToCheck = trigger switch
                {
                    AchievementTrigger.LessonCompleted  => new[] { AchievementCodes.FirstLesson, AchievementCodes.Lesson5, AchievementCodes.Lesson10, AchievementCodes.Lesson50 },
                    AchievementTrigger.CourseEnrolled   => new[] { AchievementCodes.FirstEnrollment },
                    AchievementTrigger.CourseCompleted  => new[] { AchievementCodes.FirstCourse, AchievementCodes.Course3, AchievementCodes.PerfectScore },
                    AchievementTrigger.CertificateIssued => new[] { AchievementCodes.FirstCertificate },
                    AchievementTrigger.ReviewCreated    => new[] { AchievementCodes.FirstReview },
                    _ => Array.Empty<string>()
                };

                foreach (var code in codesToCheck)
                {
                    // Уже есть — пропускаем
                    if (await _achievementRepository.HasAchievementAsync(userId, code))
                        continue;

                    // Проверяем условие
                    var earned = await CheckConditionAsync(userId, code);
                    if (!earned) continue;

                    var achievement = await _achievementRepository.GetByCodeAsync(code);
                    if (achievement == null) continue;

                    var ua = await _achievementRepository.AddUserAchievementAsync(new UserAchievement
                    {
                        UserId = userId,
                        AchievementId = achievement.Id,
                        EarnedAt = DateTime.UtcNow,
                    });

                    newlyEarned.Add(new NewAchievementDto
                    {
                        Code = achievement.Code,
                        Title = achievement.Title,
                        Description = achievement.Description,
                        Icon = achievement.Icon,
                        EarnedAt = ua.EarnedAt,
                    });

                    _logger.LogInformation("Пользователь {UserId} получил ачивку {Code}", userId, code);
                }
            }
            catch (System.Exception ex)
            {
                // Ошибка в ачивках не должна ломать основной флоу
                _logger.LogError(ex, "Ошибка при проверке ачивок userId={UserId} trigger={Trigger}", userId, trigger);
            }

            return newlyEarned;
        }

        private async Task<bool> CheckConditionAsync(Guid userId, string code)
        {
            return code switch
            {
                AchievementCodes.FirstLesson => await CountCompletedLessonsAsync(userId) >= 1,
                AchievementCodes.Lesson5     => await CountCompletedLessonsAsync(userId) >= 5,
                AchievementCodes.Lesson10    => await CountCompletedLessonsAsync(userId) >= 10,
                AchievementCodes.Lesson50    => await CountCompletedLessonsAsync(userId) >= 50,

                AchievementCodes.FirstEnrollment => await _context.Enrollments.CountAsync(e => e.UserId == userId) >= 1,

                AchievementCodes.FirstCourse => await CountCompletedCoursesAsync(userId) >= 1,
                AchievementCodes.Course3     => await CountCompletedCoursesAsync(userId) >= 3,

                AchievementCodes.PerfectScore => await _context.Enrollments
                    .AnyAsync(e => e.UserId == userId && e.ProgressPercent == 100),

                AchievementCodes.FirstCertificate => await _context.Certificates
                    .CountAsync(c => c.UserId == userId) >= 1,

                AchievementCodes.FirstReview => await _context.Reviews
                    .CountAsync(r => r.UserId == userId) >= 1,

                _ => false
            };
        }

        private Task<int> CountCompletedLessonsAsync(Guid userId)
            => _context.LessonProgresses.CountAsync(lp => lp.UserId == userId);

        private Task<int> CountCompletedCoursesAsync(Guid userId)
            => _context.Enrollments.CountAsync(e => e.UserId == userId && e.Status == EnrollmentStatus.Completed);
    }
