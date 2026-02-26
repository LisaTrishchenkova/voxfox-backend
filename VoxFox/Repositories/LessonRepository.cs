using Microsoft.EntityFrameworkCore;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories
{
    public class LessonRepository : ILessonRepository
    {
        private readonly ApplicationContext _context;
        private readonly ILogger<Lesson> _logger;

        public LessonRepository(ApplicationContext context, ILogger<Lesson> logger = null)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Lesson> AddAsync(Lesson lesson)
        {
            try
            {
                _context.Lessons.Add(lesson);
                await _context.SaveChangesAsync();

                return lesson;
            }
            catch(DbUpdateException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteAsync(Lesson lesson)
        {
            try
            {
                _context.Remove(lesson);
                await _context.SaveChangesAsync();

                return true;
            }
            catch(DbUpdateException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<Lesson?> GetByIdAsync(Guid id)
        {
            try
            {
                var lesson = await _context.Lessons
                    .FirstOrDefaultAsync(l => l.Id == id);
                
                return lesson;
            }
            catch(OperationCanceledException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }

        public Task<bool> SectionExistsAsync(Guid sectionId) => _context.Sections.AnyAsync(s => s.Id == sectionId);

        public async Task<Lesson> UpdateAsync(Lesson lesson)
        {
            try
            {
                _context.Lessons.Update(lesson);
                await _context.SaveChangesAsync();
                
                return lesson;
            }
            catch(DbUpdateException ex)
            {
                _logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
