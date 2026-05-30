namespace VoxFox.Models.DTOs.Admin;

public class AdminStatsDto
{
	public int TotalUsers { get; set; }
	public int NewUsersThisMonth { get; set; }
	public int BlockedUsers { get; set; }
	public int TotalCourses { get; set; }
	public int PublishedCourses { get; set; }
	public int PendingCourses { get; set; }
	public int DraftCourses { get; set; }
	public int TotalEnrollments { get; set; }
	public int CompletedEnrollments { get; set; }
	public int TotalCertificates { get; set; }
	public int ActiveTeachers { get; set; }
	public IList<TopCourseDto> TopCoursesByEnrollments { get; set; } = new List<TopCourseDto>();
}

public class TopCourseDto
{
	public Guid Id { get; set; }
	public string Title { get; set; } = null!;
	public string AuthorName { get; set; } = null!;
	public int EnrollmentCount { get; set; }
	public decimal Rating { get; set; }
}
