namespace VoxFox.Models.Responses
{
    public class MeResponse
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string Role { get; set; }
        public bool isEmailVerified { get; set; }
        public DateTime createdAt { get; set; }
    }
}
