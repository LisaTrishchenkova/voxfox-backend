using System.Security.Claims;

namespace VoxFox.Interfaces
{
    public interface IJwtService
    {
        string GenerateAccessToken(IEnumerable<Claim> claims);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<bool> ValidateTokenAsync(string token);
        IEnumerable<Claim> CreateClaims(Guid userId, string email);
    }
}
