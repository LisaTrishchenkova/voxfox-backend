using Microsoft.EntityFrameworkCore;
using VoxFox.Interfaces.Certificate;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories;

public class CertificateRepository : ICertificateRepository
{
	private readonly ApplicationContext _context;

	public CertificateRepository(ApplicationContext context)
	{
		_context = context;
	}

	public async Task<Certificate?> GetByIdAsync(Guid id)
		=> await _context.Certificates
			.Include(c => c.User)
			.Include(c => c.Course)
			.FirstOrDefaultAsync(c => c.Id == id);

	public async Task<Certificate?> GetByEnrollmentIdAsync(Guid enrollmentId)
		=> await _context.Certificates
			.Include(c => c.User)
			.Include(c => c.Course)
			.FirstOrDefaultAsync(c => c.EnrollmentId == enrollmentId);

	public async Task<Certificate?> GetByTokenAsync(string token)
		=> await _context.Certificates
			.Include(c => c.User)
			.Include(c => c.Course)
			.FirstOrDefaultAsync(c => c.VerificationToken == token);

	public async Task<IList<Certificate>> GetByUserIdAsync(Guid userId)
		=> await _context.Certificates
			.Where(c => c.UserId == userId)
			.Include(c => c.Course)
			.OrderByDescending(c => c.IssuedAt)
			.ToListAsync();

	public async Task<Certificate> AddAsync(Certificate certificate)
	{
		_context.Certificates.Add(certificate);
		await _context.SaveChangesAsync();
		return certificate;
	}
}
