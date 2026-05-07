namespace VoxFox.Models.DTOs;

public class UserResponseExtended
{
	public Guid Id { get; set; }
	public string Name { get; set; } = null!;
	public string Email { get; set; } = null!;
	public string? AvatarUrl { get; set; }
	public string? Bio { get; set; }
	public string Role { get; set; } = null!;
	public DateTime CreatedAt { get; set; }
	public bool IsDeleted { get; set; }
	public bool IsBlocked { get; set; }
	public DateTime? BlockedAt { get; set; }
	public string? BlockReason { get; set; }
}
