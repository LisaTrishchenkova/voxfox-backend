using VoxFox.Models.DTOs;

namespace VoxFox.Interfaces;

public interface IFavoriteService
{
	Task<ServiceResult<FavoriteDto>> AddFavoriteAsync(Guid courseId, Guid userId);
	Task<ServiceResult<bool>> RemoveFavoriteAsync(Guid courseId, Guid userId);
	Task<ServiceResult<IList<FavoriteDto>>> GetUserFavoritesAsync(Guid userId);
}
