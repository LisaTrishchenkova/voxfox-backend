using VoxFox.Enums;
using VoxFox.Interfaces;
using VoxFox.Interfaces.Enrollment;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Services.Course;

public class EnrollmentService : IEnrollmentService
{
    private readonly IEnrollmentRepository _enrollmentRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IFavoriteRepository _favoriteRepository;
    private readonly ILogger<EnrollmentService> _logger;


    public EnrollmentService(IEnrollmentRepository enrollmentRepository, ICourseRepository courseRepository, IFavoriteRepository favoriteRepository, ILogger<EnrollmentService> logger)
    {
	    _enrollmentRepository = enrollmentRepository;
	    _courseRepository = courseRepository;
	    _favoriteRepository = favoriteRepository;
	    _logger = logger;
    }

    public async Task<ServiceResult<EnrollmentDto>> EnrollAsync(Guid courseId, Guid userId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null)
            return ServiceResult<EnrollmentDto>.Fail(
                $"Курс с id: {courseId} не найден",
                StatusCodes.Status404NotFound
            );

        if (course.Status != CourseStatus.Published)
            return ServiceResult<EnrollmentDto>.Fail(
                "Записаться можно только на опубликованный курс"
            );

        var alreadyEnrolled = await _enrollmentRepository.ExistsAsync(userId, courseId);
        if (alreadyEnrolled)
            return ServiceResult<EnrollmentDto>.Fail(
                "Вы уже записаны на этот курс"
            );

        var enrollment = new Enrollment
        {
            UserId = userId,
            CourseId = courseId,
            Status = EnrollmentStatus.Active,
            EnrolledAt = DateTime.UtcNow
        };

        var created = await _enrollmentRepository.AddAsync(enrollment);

        var favorite = await _favoriteRepository.GetByUserAndCourseAsync(userId, courseId);
        if (favorite != null)
	        await _favoriteRepository.DeleteAsync(favorite);

        return ServiceResult<EnrollmentDto>.Ok(MapToDto(created));
    }

    public async Task<ServiceResult<bool>> CancelEnrollmentAsync(Guid enrollmentId, Guid userId)
    {
        var enrollment = await _enrollmentRepository.GetByIdAsync(enrollmentId);
        if (enrollment == null)
            return ServiceResult<bool>.Fail(
                $"Запись с id: {enrollmentId} не найдена",
                StatusCodes.Status404NotFound
            );

        if (enrollment.UserId != userId)
            return ServiceResult<bool>.Fail(
                "Нет доступа к этой записи",
                StatusCodes.Status403Forbidden
            );

        if (enrollment.Status == EnrollmentStatus.Completed)
            return ServiceResult<bool>.Fail(
                "Нельзя отменить завершённый курс"
            );

        await _enrollmentRepository.DeleteAsync(enrollment);
        return ServiceResult<bool>.Ok(true);
    }

    public async Task<ServiceResult<IList<EnrollmentDto>>> GetUserEnrollmentsAsync(Guid userId)
    {
        var enrollments = await _enrollmentRepository.GetByUserIdAsync(userId);
        var result = enrollments.Select(MapToDtoWithCourse).ToList();
        return ServiceResult<IList<EnrollmentDto>>.Ok(result);
    }

    public async Task<ServiceResult<IList<EnrollmentDto>>> GetCourseEnrollmentsAsync(Guid courseId, Guid requesterId,
	    UserRole? requesterRole)
    {
	    var course = await _courseRepository.GetByIdAsync(courseId);
	    if (course == null)
	    {
		    return ServiceResult<IList<EnrollmentDto>>.Fail(
			    $"Курс с id: {courseId} не найден",
			    StatusCodes.Status404NotFound);
	    }

	    var isAdminOrModerator = requesterRole is UserRole.Admin or UserRole.Moderator;
	    if (!isAdminOrModerator && course.AuthorId != requesterId)
		    return ServiceResult<IList<EnrollmentDto>>.Fail(
			    "Нет доступа — вы не являетесь автором курса",
			    StatusCodes.Status403Forbidden);

	    var enrollments = await _enrollmentRepository.GetByCourseIdAsync(courseId);

	    var resualt = enrollments.Select(e => new EnrollmentDto
	    {
		    Id = e.Id,
		    UserId = e.UserId,
		    CourseId = e.CourseId,
		    Status = e.Status,
		    ProgressPercent = e.ProgressPercent,
		    EnrolledAt = e.EnrolledAt,
		    CompletedAt = e.CompletedAt
	    }).ToList();

	    return ServiceResult<IList<EnrollmentDto>>.Ok(resualt);
    }

    private EnrollmentDto MapToDto(Enrollment enrollment)
    {
        return new EnrollmentDto
        {
            Id = enrollment.Id,
            UserId = enrollment.UserId,
            CourseId = enrollment.CourseId,
            Status = enrollment.Status,
            ProgressPercent = enrollment.ProgressPercent,
            EnrolledAt = enrollment.EnrolledAt,
            CompletedAt = enrollment.CompletedAt
        };
    }

    private EnrollmentDto MapToDtoWithCourse(Enrollment enrollment)
    {
        return new EnrollmentDto
        {
            Id = enrollment.Id,
            UserId = enrollment.UserId,
            CourseId = enrollment.CourseId,
            Status = enrollment.Status,
            ProgressPercent = enrollment.ProgressPercent,
            EnrolledAt = enrollment.EnrolledAt,
            CompletedAt = enrollment.CompletedAt,
            Course = enrollment.Course == null ? null : new CourseDto
            {
                Id = enrollment.Course.Id,
                Title = enrollment.Course.Title,
                Description = enrollment.Course.Description,
                Status = enrollment.Course.Status,
                Level = enrollment.Course.Level,
                CoverImageUrl = enrollment.Course.CoverImageUrl,
                Price = enrollment.Course.Price,
                CertificateEnabled = enrollment.Course.CertificateEnabled,
                EnrollmentCount = enrollment.Course.EnrollmentCount,
                Rating = enrollment.Course.Rating,
                DurationMinutes = enrollment.Course.DurationMinutes,
                PublishedAt = enrollment.Course.PublishedAt,
                CreatedAt = enrollment.Course.CreatedAt
            }
        };
    }
}
