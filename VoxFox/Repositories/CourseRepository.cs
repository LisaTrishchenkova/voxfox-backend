using System.Runtime.InteropServices.Marshalling;
using Microsoft.EntityFrameworkCore;
using VoxFox.Models.DTOs;
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

            await _context.Entry(course)
              .Reference(c => c.Author)  // Reference - для одиночной связи
              .LoadAsync();

            return course;
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex.StackTrace);
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
                .Include(c => c.Tags)
                .Include(c => c.Author)
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
                .Include(c => c.Author)
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

    public IQueryable<Course> GetCoursesQuery()
    {
        return _context.Courses
             // .Include(c => c.Category)
             .AsNoTracking()
             .AsQueryable();
    }

    public IQueryable<Course> GetPublishedCoursesQuery()
    {
        return _context.Courses
             .Where(c => c.Status == VoxFox.Enums.CourseStatus.Published)
             .AsNoTracking()
             .Include(c => c.Author)
             .AsQueryable();
    }
    public async Task<List<CourseDto>> GetCoursesWithProjectionAsync(IQueryable<Course> query, int skip, int take)
    {
        return await query
               .Skip(skip)
               .Take(take)
               .Select(c => new CourseDto
               {
                   Id = c.Id,
                   Title = c.Title,
                   Description = c.Description,
                   Status = c.Status,
                   Tags = c.Tags != null ? c.Tags.Select(t => new TagDto
                   {
                       Name = t.Name
                   }).ToList() : new List<TagDto>(),
                   // Price = c.Price,
                   CategoryId = c.CategoryId,
                   Author = new AuthorDto
                  {
                   Id = c.Author.Id,
                   Name = c.Author.Name
                    },
                   PublishedAt = c.PublishedAt
               })
               .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync(IQueryable<Course> query)
    {
        return await query.CountAsync();
    }

    public async Task DeleteTagAsync(Tag tag)
    {
        _context.Tags.Remove(tag);
        await _context.SaveChangesAsync();
    }
}
