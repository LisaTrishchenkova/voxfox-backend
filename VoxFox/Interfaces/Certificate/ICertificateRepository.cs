namespace VoxFox.Interfaces.Certificate;

public interface ICertificateRepository
{
	Task<Models.Entities.Certificate?> GetByIdAsync(Guid id);
	Task<Models.Entities.Certificate?> GetByEnrollmentIdAsync(Guid enrollmentId);
	Task<Models.Entities.Certificate?> GetByTokenAsync(string token);
	Task<IList<Models.Entities.Certificate>> GetByUserIdAsync(Guid userId);
	Task<Models.Entities.Certificate> AddAsync(Models.Entities.Certificate certificate);
}
