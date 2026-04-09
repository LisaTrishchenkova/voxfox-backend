using VoxFox.Interfaces;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Services;

public class FavoriteService : IFavoriteService
{
	private readonly IFavoriteRepository _favoriteRepository;
	private readonly ICourseRepository _courseRepository;
	private readonly ILogger<FavoriteService> _logger;

	public FavoriteService(IFavoriteRepository favoriteRepository, ICourseRepository courseRepository, ILogger<FavoriteService> logger)
	{
		_favoriteRepository = favoriteRepository;
		_courseRepository = courseRepository;
		_logger = logger;
	}

	public async Task<ServiceResult<FavoriteDto>> AddFavoriteAsync(Guid courseId, Guid userId)
	{
		var course = await _courseRepository.GetByIdAsync(courseId);
		if (course == null)
			return ServiceResult<FavoriteDto>.Fail(
				$"Курс с id: {courseId} не найден",
				StatusCodes.Status404NotFound
			);

		var existing = await _favoriteRepository.GetByUserAndCourseAsync(userId, courseId);
		if (existing != null)
			return ServiceResult<FavoriteDto>.Fail(
				"Курс уже в избранном",
				StatusCodes.Status400BadRequest
			);

		var favorite = new Favorite
		{
			UserId = userId,
			CourseId = courseId,
		};

		var created = await _favoriteRepository.AddAsync(favorite);
		return ServiceResult<FavoriteDto>.Ok(MapToDto(created));
	}

	public async Task<ServiceResult<bool>> RemoveFavoriteAsync(Guid courseId, Guid userId)
	{
		var favorite = await _favoriteRepository.GetByUserAndCourseAsync(userId, courseId);
		if (favorite == null)
			return ServiceResult<bool>.Fail(
				"Курс не найден в избранном",
				StatusCodes.Status404NotFound
			);

		var result = await _favoriteRepository.DeleteAsync(favorite);
		return result
			? ServiceResult<bool>.Ok(true)
			: ServiceResult<bool>.Fail("Не удалось удалить из избранного");
	}

	public async Task<ServiceResult<IList<FavoriteDto>>> GetUserFavoritesAsync(Guid userId)
	{
		var favorites = await _favoriteRepository.GetByUserIdAsync(userId);
		var result = favorites.Select(MapToDtoWithCourse).ToList();
		return ServiceResult<IList<FavoriteDto>>.Ok(result);
	}
	private static FavoriteDto MapToDto(Favorite favorite) => new()
	{
		Id = favorite.Id,
		CourseId = favorite.CourseId,
		CreatedAt = favorite.CreatedAt,
	};

	private static FavoriteDto MapToDtoWithCourse(Favorite favorite) => new()
	{
		Id = favorite.Id,
		CourseId = favorite.CourseId,
		CreatedAt = favorite.CreatedAt,
		Course = new CourseDto
		{
			Id = favorite.Course.Id,
			Title = favorite.Course.Title,
			Description = favorite.Course.Description,
			Status = favorite.Course.Status,
			Level = favorite.Course.Level,
			CoverImageUrl = favorite.Course.CoverImageUrl,
			Price = favorite.Course.Price,
			CertificateEnabled = favorite.Course.CertificateEnabled,
			EnrollmentCount = favorite.Course.EnrollmentCount,
			Rating = favorite.Course.Rating,
			DurationMinutes = favorite.Course.DurationMinutes,
			PublishedAt = favorite.Course.PublishedAt,
			CreatedAt = favorite.Course.CreatedAt,
			Author = favorite.Course.Author == null ? null : new AuthorDto
			{
				Id = favorite.Course.Author.Id,
				Name = favorite.Course.Author.Name
			},
			Tags = favorite.Course.Tags?.Select(t => new TagDto { Name = t.Name }).ToList()
		}
	};
}
