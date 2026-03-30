using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Controllers
{
    [ApiController]
    [Route("api/Categories")]
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
            var existingCategory = await _context.Categories
                .AnyAsync(c => string.Equals(c.Name, createCategoryDto.Name, StringComparison.OrdinalIgnoreCase));

            if (existingCategory)
            {
                return BadRequest($"Категория с названием '{createCategoryDto.Name}' уже существует");
            }

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

        [HttpPut("{id}")]
        public async Task<ActionResult<CategoryDto>> UpdateCategory(Guid id, CreateCategoryDto updateCategoryDto)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound($"Категория с ID {id} не найдена");
            }

            var existingCategory = await _context.Categories
                .AnyAsync(c => string.Equals(c.Name, updateCategoryDto.Name, StringComparison.OrdinalIgnoreCase) && c.Id != id);

            if (existingCategory)
            {
                return BadRequest($"Категория с названием '{updateCategoryDto.Name}' уже существует");
            }

            category.Name = updateCategoryDto.Name;
            await _context.SaveChangesAsync();

            var categoryDto = new CategoryDto
            {
                Id = category.Id,
                Name = category.Name
            };

            return Ok(categoryDto);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound($"Категория с ID {id} не найдена");
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
