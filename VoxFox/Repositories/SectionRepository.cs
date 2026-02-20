using System.Collections;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories
{
    public class SectionRepository : ISectionRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<SectionRepository> _logger;

        public SectionRepository(ApplicationContext context, ILogger<SectionRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Section> AddAsync(Section section)
        {
            try
            {
                _context.Sections.Add(section);
                await _context.SaveChangesAsync();

                return section;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public Task<bool> CourseExistsAsync(Guid courseId) => _context.Courses.AnyAsync(c => c.Id == courseId);

        public async Task<bool> DeleteAsync(Section section)
        {
            try
            {
                _context.Remove(section);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<IList<Section>?> GetAllAsync()
        {
            try
            {
                var sections = await _context.Sections
                    .AsNoTracking()
                    .ToListAsync();

                return sections;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<IList<Section>> GetByCourseIdAsync(Guid courseId)
        {
            try
            {
                var courses = await _context.Sections
                    .Where(s => s.CourseId == courseId)
                    .ToListAsync();

                return courses;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }

        }

        public async Task<Section> UpdateAsync(Section section)
        {
            try
            {
                _context.Sections.Update(section);
                await _context.SaveChangesAsync();

                return section;
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        async Task<Section?> ISectionRepository.GetByIdAsync(Guid id)
        {
            try
            {
                var course = await _context.Sections
                    .FirstOrDefaultAsync(s => s.Id == id);

                return course;
            }
            catch (OperationCanceledException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
