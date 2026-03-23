namespace VoxFox.Models.Responses.AuthResponse
{
    public class LoginResponse
    {
        public Guid UserId { get; set; }

        public required string TokenAccess { get; set; }

        public string? TokenRefresh { get; set; }
    }
}
