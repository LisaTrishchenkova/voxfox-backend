using Microsoft.EntityFrameworkCore;
using VoxFox.Models.Entities;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationContext _context;
    public CourseRepository(ApplicationContext context)
    {
        _context = context;
    }
    public async Task<Course> AddAsync(Course course)
    {
        _context.Courses.Add(course);
        await _context.SaveChangesAsync();
        return course;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var course = await _context.Courses.FindAsync(id);
        if (course == null)
            return false;
        _context.Courses.Remove(course);
        return true;
    }

    public async Task<IList<Course>> GetAllAsync()
    {
        var courses = await _context.Courses
            .AsNoTracking()
            .ToListAsync();
        return courses;
    }

    public async Task<Course> GetByIdAsync(Guid id)
    {
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
        if(course == null)
        {
            return null;
        }
        return course;
    }

    public async Task<Course> UpdateAsync(Course course)
    {
        var existingCourse = await _context.Courses.FindAsync(course.Id);
        if (existingCourse == null)
            return null;
        existingCourse.Title = course.Title;
        existingCourse.Description = course.Description;
        existingCourse.Tags = course.Tags;
        await _context.SaveChangesAsync();
        return existingCourse;
    }
}
