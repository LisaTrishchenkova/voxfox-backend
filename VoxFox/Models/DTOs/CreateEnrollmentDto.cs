using System.ComponentModel.DataAnnotations;

namespace VoxFox.Models.DTOs;

public class CreateEnrollmentDto
{
	[Required]
	public Guid CourseId { get; set; }

}
