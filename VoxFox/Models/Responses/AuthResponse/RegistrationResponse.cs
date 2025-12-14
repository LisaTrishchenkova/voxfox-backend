namespace VoxFox.Models.Responses
{
    public class RegistrationResponse
    {
        public Guid UserId { get; set; }

        public string TokenAccess { get; set; }

        public string TokenRefresh { get; set; }
    }
}
