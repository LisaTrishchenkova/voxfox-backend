namespace VoxFox.Models.Responses.UserResponse
{
    public class UserResponse
    {
	    public Guid Id { get; set; }
	    public string? Name { get; set; }
	    public string? Email { get; set; }
	    public string? AvatarUrl { get; set; }
	    public string? Bio { get; set; }
	    public string? Role { get; set; }
	    public DateTime CreatedAt { get; set; }
	    public bool IsDeleted { get; set; }
    }
}
