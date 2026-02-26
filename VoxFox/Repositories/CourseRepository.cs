using System.Runtime.InteropServices.Marshalling;
using Microsoft.EntityFrameworkCore;
using VoxFox.Models.Entities;

public class CourseRepository : ICourseRepository
{
    private readonly ApplicationContext _context;
    private readonly ILogger<CourseRepository> _logger;

    public CourseRepository(ApplicationContext context, ILogger<CourseRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Course> AddAsync(Course course)
    {
        try
        {
            _context.Courses.Add(course);
            await _context.SaveChangesAsync();

            return course;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task<bool> DeleteSoftAsync(Course course)
    {
        try
        {
            course.IsDeleted = true;

            _context.Courses.Update(course);
            await _context.SaveChangesAsync();

            return true;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }

    }

    public Task<bool> ExistCourseByIdAsync(Guid id) => _context.Courses.AnyAsync(c => c.Id == id);

    public async Task<IList<Course>?> GetAllAsync()
    {
        try
        {
            var courses = await _context.Courses
                .AsNoTracking()
                .ToListAsync();

            return courses;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        try
        {
            var course = await _context.Courses
                .Include(c => c.Tags)
                .FirstOrDefaultAsync(c => c.Id == id);

            return course;
        }
        catch (OperationCanceledException ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task<Course> UpdateAsync(Course course)
    {
        try
        {
            _context.Courses.Update(course);
            await _context.SaveChangesAsync();

            return course;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex.Message);
            throw;
        }
    }

    public async Task<IList<Section>> GetSectionsByCourseIdAsync(Guid courseId)
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
}
