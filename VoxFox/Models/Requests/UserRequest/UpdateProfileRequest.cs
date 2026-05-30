using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.Requests.UserRequest;

public class UpdateProfileRequest
{
	[StringLength(100, MinimumLength = 2)]
	public string? Name { get; set; }

	[StringLength(500)]
	public string? Bio { get; set; }
}
