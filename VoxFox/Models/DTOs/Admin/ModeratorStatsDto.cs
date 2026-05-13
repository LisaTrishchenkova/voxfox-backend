namespace VoxFox.Models.DTOs.Admin;

public class ModeratorStatsDto
{
	public Guid ModeratorId { get; set; }
	public string ModeratorName { get; set; } = null!;
	public int TotalReviewed { get; set; }
	public int TotalApproved { get; set; }
	public int TotalRejected { get; set; }
	public int CurrentlyReviewing { get; set; }
}
