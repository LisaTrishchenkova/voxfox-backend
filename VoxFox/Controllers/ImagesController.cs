using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VoxFox.Models.Entities;

namespace VoxFox.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ImagesController : ControllerBase
	{
		private readonly ApplicationContext _context;

		public ImagesController(ApplicationContext context)
		{
			_context = context;
		}

		[HttpPost]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> Upload(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest(new { error = "Файл не выбран" });

			if (file.Length > 5 * 1024 * 1024)
				return BadRequest(new { error = "Файл не должен превышать 5MB" });

			using var ms = new MemoryStream();
			await file.CopyToAsync(ms);

			var image = new Image
			{
				ImageBytes = ms.ToArray()
			};

			_context.Images.Add(image);
			await _context.SaveChangesAsync();

			return Ok(new { url = $"/api/images/{image.Id}" });
		}

		[HttpGet("{id}")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<IActionResult> Get([FromRoute] Guid id)
		{
			var image = await _context.Images.FirstOrDefaultAsync(i => i.Id == id);
			if (image == null)
				return NotFound();

			return File(image.ImageBytes, "image/jpeg");
		}
	}
}
