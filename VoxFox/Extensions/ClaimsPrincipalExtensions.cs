using System.Security.Claims;

namespace VoxFox.Extensions;

public static class ClaimsPrincipalExtensions
{
		public static Guid? GetUserId(this ClaimsPrincipal user)
		{
			var value = user.FindFirstValue(ClaimTypes.NameIdentifier);
			return Guid.TryParse(value, out var id) ? id : null;
		}
}
