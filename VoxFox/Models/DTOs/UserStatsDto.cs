namespace VoxFox.Models.DTOs;

public class UserStatsDto
{
	//для студента
	public int EnrolledCoursesCount { get; set; }
	public int CompletedCoursesCount { get; set; }
	public int InProgressCoursesCount { get; set; }
	public int TotalScore { get; set; }

	//для учителя
	public int CreatedCoursesCount { get; set; }
	public int PublishedCoursesCount { get; set; }
	public int TotalStudentsCount { get; set; }
	public double AverageRating { get; set; }
}
