using VoxFox.Models.DTOs;

namespace VoxFox.Interfaces.Teacher;

public interface ITeacherRepository
{
	/// <summary>Курсы преподавателя с базовыми данными для статистики</summary>
	Task<List<TeacherCourseStatsDto>> GetCourseStatsAsync(Guid teacherId);

	/// <summary>Общий заработок преподавателя (все Earning-транзакции)</summary>
	Task<decimal> GetTotalEarningsAsync(Guid teacherId);

	/// <summary>Заработок за текущий месяц</summary>
	Task<decimal> GetEarningsThisMonthAsync(Guid teacherId);

	/// <summary>Число сертификатов по всем курсам преподавателя</summary>
	Task<int> GetTotalCertificatesAsync(Guid teacherId);

	/// <summary>Число завершённых enrollment по всем курсам</summary>
	Task<int> GetCompletedEnrollmentsAsync(Guid teacherId);

	/// <summary>Заработок по конкретному курсу</summary>
	Task<decimal> GetCourseEarningsAsync(Guid teacherId, Guid courseId);
}
