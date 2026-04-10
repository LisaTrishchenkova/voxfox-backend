namespace VoxFox.Models.Responses.AuthResponse
{
    public class MeResponse
    {
        public Guid Id { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? AvatarUrl { get; set; } 
    }
}
