using System.ComponentModel.DataAnnotations;
using VoxFox.Enums;

namespace VoxFox.Models.DTOs;

public class SetRoleRequest
{
	[Required]
	[EnumDataType(typeof(UserRole))]
	public UserRole Role { get; set; }
}
