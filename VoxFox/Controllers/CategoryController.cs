using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;
namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoryController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public CategoryController(ApplicationContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryDto>>> GetAllCategories()
        {
            var categories = await _context.Categories
                .Select(c => new CategoryDto
                {
                    Id = c.Id,
                    Name = c.Name,
                })
                .ToListAsync();

            return Ok(categories);
        }
        [HttpPost]
        public async Task<ActionResult<CategoryDto>> CreateCategory(CreateCategoryDto createCategoryDto)
        {
            var category = new Category
            {
                Name = createCategoryDto.Name
            };

            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };

            return CreatedAtAction(nameof(GetAllCategories), new { id = category.Id }, categoryDto);
        }
    }
}
