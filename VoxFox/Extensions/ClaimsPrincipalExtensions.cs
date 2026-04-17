using System.Security.Claims;
using VoxFox.Enums;

namespace VoxFox.Extensions;

public static class ClaimsPrincipalExtensions
{
		public static Guid? GetUserId(this ClaimsPrincipal user)
		{
			var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
			return Guid.TryParse(value, out var id) ? id : null;
		}
		public static UserRole? GetUserRole(this ClaimsPrincipal user)
		{
			var value = user.FindFirstValue(ClaimTypes.Role);
			return Enum.TryParse<UserRole>(value, out var role) ? role : null;
		}
}
