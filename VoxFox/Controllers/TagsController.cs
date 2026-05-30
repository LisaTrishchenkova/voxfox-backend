using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoxFox.Models.Entities;

namespace VoxFox.Controllers;

[ApiController]
[Route("api/Tags")]
public class TagsController : ControllerBase
{
	private readonly ApplicationContext _context;

	public TagsController(ApplicationContext context)
	{
		_context = context;
	}

	[HttpGet]
	[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IList<string>))]
	public async Task<ActionResult<IList<string>>> GetAllTags()
	{
		var tags = await _context.Tags
			.Select(t => t.Name)
			.Distinct()
			.OrderBy(name => name)
			.ToListAsync();

		return Ok(tags);
	}

	[HttpGet("popular")]
	[ProducesResponseType(StatusCodes.Status200OK)]
	public async Task<ActionResult> GetPopularTags([FromQuery] int limit = 20)
	{
		var tags = await _context.Tags
			.GroupBy(t => t.Name)
			.Select(g => new
			{
				name = g.Key,
				courseCount = g.Count()
			})
			.OrderByDescending(t => t.courseCount)
			.Take(limit)
			.ToListAsync();

		return Ok(tags);
	}
}
