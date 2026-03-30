using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoxFox.Models.DTOs;
using VoxFox.Models.Entities;

namespace VoxFox.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorsController : ControllerBase
{
	private readonly ApplicationContext _context;

	public AuthorsController(ApplicationContext context)
	{
		_context = context;
	}

	[HttpGet]
	public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAllAuthors()
	{
		var authors = await _context.Authors
			.Select(c => new AuthorDto
			{
				Id = c.Id,
				Name = c.Name
			})
			.ToListAsync();

		return Ok(authors);
	}

	[Authorize]
	[HttpPost]
	public async Task<ActionResult<AuthorDto>> CreateAuthor(CreateAuthorDto createAuthorDto)
	{
		var existingAuthor = await _context.Authors
			.AnyAsync(c => EF.Functions.ILike(c.Name, createAuthorDto.Name));

		if (existingAuthor) return BadRequest($"Автор с именем '{createAuthorDto.Name}' уже существует");

		var author = new Author
		{
			Name = createAuthorDto.Name
		};

		_context.Authors.Add(author);
		await _context.SaveChangesAsync();

		var authorDto = new AuthorDto
		{
			Id = author.Id,
			Name = author.Name
		};

		return CreatedAtAction(nameof(GetAllAuthors), new { id = author.Id }, authorDto);
	}

	[HttpPut("{id}")]
	public async Task<ActionResult<AuthorDto>> UpdateAuthor(Guid id, CreateAuthorDto updateAuthorDto)
	{
		var author = await _context.Authors.FindAsync(id);

		if (author == null) return NotFound($"Автор с ID {id} не найдена");

		var existingAuthor = await _context.Categories
			.AnyAsync(c =>
				string.Equals(c.Name, updateAuthorDto.Name, StringComparison.OrdinalIgnoreCase) && c.Id != id);

		if (existingAuthor) return BadRequest($"Автор с именем '{updateAuthorDto.Name}' уже существует");

		author.Name = updateAuthorDto.Name;
		await _context.SaveChangesAsync();

		var authorDto = new AuthorDto
		{
			Id = author.Id,
			Name = author.Name
		};

		return Ok(authorDto);
	}

	[HttpDelete("{id}")]
	public async Task<IActionResult> DeleteAuthor(Guid id)
	{
		var author = await _context.Authors.FindAsync(id);

		if (author == null) return NotFound($"Автор с ID {id} не найдена");

		_context.Authors.Remove(author);
		await _context.SaveChangesAsync();

		return NoContent();
	}
}
