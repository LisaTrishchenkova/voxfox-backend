using Microsoft.EntityFrameworkCore;
using VoxFox.Interfaces.Lesson;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories
{
    public class LessonRepository(ApplicationContext context, ILogger<Lesson> logger) : ILessonRepository
    {
        public async Task<Lesson> AddAsync(Lesson lesson)
        {
            try
            {
                context.Lessons.Add(lesson);
                await context.SaveChangesAsync();

                return lesson;
            }
            catch(DbUpdateException ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<bool> DeleteSoftAsync(Lesson lesson)
        {
            try
            {

                lesson.IsDeleted = true;

                context.Lessons.Update(lesson);
                await context.SaveChangesAsync();

                return true;
            }
            catch(DbUpdateException ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }

        public async Task<Lesson?> GetByIdAsync(Guid id)
        {
            try
            {
                var lesson = await context.Lessons
                    .FirstOrDefaultAsync(l => l.Id == id);

                return lesson;
            }
            catch(OperationCanceledException ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }

        public Task<bool> SectionExistsAsync(Guid sectionId) => context.Sections.AnyAsync(s => s.Id == sectionId);
        public async Task<Lesson?> GetByIdWithSectionAsync(Guid id)
        {
	        return await context.Lessons
		        .Include(l => l.Section)
		        .ThenInclude(s => s.Course)
		        .ThenInclude(c => c!.Author)
		        .FirstOrDefaultAsync(l => l.Id == id);
        }

        public async Task<Lesson> UpdateAsync(Lesson lesson)
        {
            try
            {
                context.Lessons.Update(lesson);
                await context.SaveChangesAsync();

                return lesson;
            }
            catch(DbUpdateException ex)
            {
                logger.LogError(ex.Message);
                throw;
            }
        }
    }
}
