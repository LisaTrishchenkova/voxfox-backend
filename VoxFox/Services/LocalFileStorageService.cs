// using VoxFox.Interfaces.User;
//
// namespace VoxFox.Services;
//
// public class LocalFileStorageService : IFileStorageService
// {
//     private readonly IWebHostEnvironment _env;
//
//     public LocalFileStorageService(IWebHostEnvironment env)
//     {
//         _env = env;
//     }
//
//     private string GetWebRootPath() =>
//         _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
//
//     public async Task<ServiceResult<string>> SaveAvatarAsync(Guid userId, IFormFile file)
//     {
//         var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
//         var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
//         if (!allowed.Contains(ext))
//             return ServiceResult<string>.Fail("Недопустимый формат файла", StatusCodes.Status400BadRequest);
//
//         var avatarsPath = Path.Combine(GetWebRootPath(), "avatars");
//         Directory.CreateDirectory(avatarsPath);
//
//         foreach (var old in Directory.GetFiles(avatarsPath, $"{userId}.*"))
//             File.Delete(old);
//
//         var fileName = $"{userId}{ext}";
//         var fullPath = Path.Combine(avatarsPath, fileName);
//
//         await using var stream = new FileStream(fullPath, FileMode.Create);
//         await file.CopyToAsync(stream);
//
//         return ServiceResult<string>.Ok($"/avatars/{fileName}");
//     }
//
//     public Task<ServiceResult<bool>> DeleteAvatarAsync(string url)
//     {
//         var fullPath = Path.Combine(GetWebRootPath(), "avatars", Path.GetFileName(url));
//
//         if (!File.Exists(fullPath))
//             return Task.FromResult(ServiceResult<bool>.Fail("Файл не найден", StatusCodes.Status404NotFound));
//
//         File.Delete(fullPath);
//         return Task.FromResult(ServiceResult<bool>.Ok(true));
//     }
//
//     public async Task<ServiceResult<string>> SaveCourseCoverAsync(Guid courseId, IFormFile file)
//     {
//         var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
//         var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
//         if (!allowed.Contains(ext))
//             return ServiceResult<string>.Fail("Недопустимый формат файла. Разрешены: jpg, jpeg, png, webp", StatusCodes.Status400BadRequest);
//
//         if (file.Length > 10 * 1024 * 1024)
//             return ServiceResult<string>.Fail("Файл не должен превышать 10MB", StatusCodes.Status400BadRequest);
//
//         var coversPath = Path.Combine(GetWebRootPath(), "covers");
//         Directory.CreateDirectory(coversPath);
//
//         // Удаляем старую обложку если есть
//         foreach (var old in Directory.GetFiles(coversPath, $"{courseId}.*"))
//             File.Delete(old);
//
//         var fileName = $"{courseId}{ext}";
//         var fullPath = Path.Combine(coversPath, fileName);
//
//         await using var stream = new FileStream(fullPath, FileMode.Create);
//         await file.CopyToAsync(stream);
//
//         return ServiceResult<string>.Ok($"/covers/{fileName}");
//     }
//
//     public Task<ServiceResult<bool>> DeleteCourseCoverAsync(string url)
//     {
//         var fullPath = Path.Combine(GetWebRootPath(), "covers", Path.GetFileName(url));
//
//         if (!File.Exists(fullPath))
//             return Task.FromResult(ServiceResult<bool>.Fail("Файл не найден", StatusCodes.Status404NotFound));
//
//         File.Delete(fullPath);
//         return Task.FromResult(ServiceResult<bool>.Ok(true));
//     }
// }
