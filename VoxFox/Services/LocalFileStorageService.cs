using VoxFox.Interfaces.User;

namespace VoxFox.Services;

public class LocalFileStorageService : IFileStorageService
{
	private readonly IWebHostEnvironment _env;

	public LocalFileStorageService(IWebHostEnvironment env)
	{
		_env = env;
	}

	public async Task<ServiceResult<string>> SaveAvatarAsync(Guid userId, IFormFile file)
	{
		var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
		var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
		if (!allowed.Contains(ext))
			return ServiceResult<string>.Fail(
				"Недопустимый формат файла",
				StatusCodes.Status400BadRequest
			);

		var webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
		var avatarsPath = Path.Combine(webRootPath, "avatars");
		Directory.CreateDirectory(avatarsPath);

		foreach (var old in Directory.GetFiles(avatarsPath, $"{userId}.*"))
			File.Delete(old);

		var fileName = $"{userId}{ext}";
		var fullPath = Path.Combine(avatarsPath, fileName);

		await using var stream = new FileStream(fullPath, FileMode.Create);
		await file.CopyToAsync(stream);

		return ServiceResult<string>.Ok($"/avatars/{fileName}");
	}

	public Task<ServiceResult<bool>> DeleteAvatarAsync(string url)
	{
		var webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
		var fileName = Path.GetFileName(url);
		var fullPath = Path.Combine(webRootPath, "avatars", fileName);

		if (!File.Exists(fullPath))
			return Task.FromResult(ServiceResult<bool>.Fail(
				"Файл не найден",
				StatusCodes.Status404NotFound
			));

		File.Delete(fullPath);
		return Task.FromResult(ServiceResult<bool>.Ok(true));
	}
}
