namespace VoxFox.Models.Responses.AuthResponse
{
    public class RefreshResponse
    {
        public Guid UserId { get; set; }

        public string? TokenAccess { get; set; }

        public string? TokenRefresh { get; set; }
    }
}
