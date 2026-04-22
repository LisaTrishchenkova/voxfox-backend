using Microsoft.EntityFrameworkCore;
using VoxFox.Interfaces.Lesson;
using VoxFox.Models.Entities;

namespace VoxFox.Repositories;

public class LessonProgressRepository : ILessonProgressRepository
{
    private readonly ApplicationContext _context;

    public LessonProgressRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<LessonProgress?> GetAsync(Guid userId, Guid lessonId)
    {
        return await _context.LessonProgresses
            .FirstOrDefaultAsync(p => p.UserId == userId && p.LessonId == lessonId);
    }

    public async Task<LessonProgress> AddAsync(LessonProgress progress)
    {
        _context.LessonProgresses.Add(progress);
        await _context.SaveChangesAsync();
        return progress;
    }

    public async Task<int> CountCompletedAsync(Guid enrollmentId)
    {
        return await _context.LessonProgresses
            .CountAsync(p => p.EnrollmentId == enrollmentId);
    }

    public async Task<int> CountTotalLessonsInCourseAsync(Guid courseId)
    {
        return await _context.Lessons
            .Where(l => l.Section.CourseId == courseId)
            .CountAsync();
    }

}
