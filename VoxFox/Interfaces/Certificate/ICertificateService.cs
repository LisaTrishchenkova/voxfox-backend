using VoxFox.Models.DTOs.Certificate;

namespace VoxFox.Interfaces.Certificate;

public interface ICertificateService
{
	Task<Models.Entities.Certificate?> IssueCertificateAsync(Guid userId, Guid courseId, Guid enrollmentId, bool certificateEnabled);
	Task<ServiceResult<IList<CertificateDto>>> GetMyCertificatesAsync(Guid userId);
	Task<ServiceResult<CertificateDto>> GetByIdAsync(Guid id, Guid userId);
	Task<ServiceResult<CertificateDto>> VerifyAsync(string token);
	Task<ServiceResult<byte[]>> GeneratePdfAsync(Guid id, Guid userId);
}
