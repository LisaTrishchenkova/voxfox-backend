namespace VoxFox.Models.DTOs;

public class UserListItemDto
{
	public Guid Id { get; set; }
	public string Name { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string Role { get; set; } = null!;
	public string? AvatarUrl { get; set; }
	public DateTime CreatedAt { get; set; }
	public bool IsDeleted { get; set; }
}
