namespace VoxFox.Models.DTOs;

public class TeacherStatsDto
{
	/// <summary>Общее число студентов по всем курсам</summary>
	public int TotalStudents { get; set; }

	/// <summary>Число опубликованных курсов</summary>
	public int PublishedCourses { get; set; }

	/// <summary>Число курсов всего (включая черновики)</summary>
	public int TotalCourses { get; set; }

	/// <summary>Средний рейтинг по всем курсам (взвешенный по числу отзывов)</summary>
	public decimal AverageRating { get; set; }

	/// <summary>Общий заработок — сумма Earning-транзакций</summary>
	public decimal TotalEarnings { get; set; }

	/// <summary>Заработок за текущий месяц</summary>
	public decimal EarningsThisMonth { get; set; }

	/// <summary>Число выданных сертификатов по всем курсам</summary>
	public int TotalCertificates { get; set; }

	/// <summary>Число завершённых enrollment по всем курсам</summary>
	public int CompletedEnrollments { get; set; }
}
public class TeacherCourseStatsDto
{
	public Guid CourseId { get; set; }
	public string Title { get; set; } = null!;
	public string? CoverImageUrl { get; set; }
	public string Status { get; set; } = null!;
	public decimal Price { get; set; }

	/// <summary>Число активных студентов (enrollment со статусом Active)</summary>
	public int ActiveStudents { get; set; }

	/// <summary>Число завершивших курс (enrollment со статусом Completed)</summary>
	public int CompletedStudents { get; set; }

	/// <summary>Всего записей</summary>
	public int TotalStudents { get; set; }

	/// <summary>Средний прогресс студентов в процентах</summary>
	public decimal AverageProgress { get; set; }

	/// <summary>Рейтинг курса</summary>
	public decimal Rating { get; set; }

	/// <summary>Число отзывов</summary>
	public int ReviewCount { get; set; }

	/// <summary>Заработок по этому курсу — сумма Earning Amount по транзакциям</summary>
	public decimal Earnings { get; set; }

	/// <summary>Число выданных сертификатов</summary>
	public int CertificatesIssued { get; set; }

	public DateTime? PublishedAt { get; set; }
	public DateTime CreatedAt { get; set; }
}
