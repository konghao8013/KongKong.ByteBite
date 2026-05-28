using ByteBite.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ByteBite.Api.Controllers;

[ApiController]
[Route("api/files")]
public class FilesController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };

    private readonly IWebHostEnvironment _environment;

    public FilesController(IWebHostEnvironment environment) { _environment = environment; }

    [HttpPost("upload")]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<object> Upload([FromForm] IFormFile file, CancellationToken ct)
    {
        if (file.Length == 0) throw new BusinessException(400, "请选择要上传的文件");
        if (file.Length > 5 * 1024 * 1024) throw new BusinessException(400, "文件不能超过5MB");

        var extension = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(extension)) throw new BusinessException(400, "仅支持 jpg、png、webp、gif 图片");

        var webRoot = string.IsNullOrWhiteSpace(_environment.WebRootPath)
            ? Path.Combine(_environment.ContentRootPath, "wwwroot")
            : _environment.WebRootPath;
        var dayFolder = DateTime.UtcNow.ToString("yyyyMMdd");
        var folder = Path.Combine(webRoot, "uploads", dayFolder);
        Directory.CreateDirectory(folder);

        var fileName = $"{Guid.NewGuid():N}{extension.ToLowerInvariant()}";
        var filePath = Path.Combine(folder, fileName);
        await using (var stream = System.IO.File.Create(filePath))
        {
            await file.CopyToAsync(stream, ct);
        }

        var relativeUrl = $"/uploads/{dayFolder}/{fileName}";
        return new
        {
            Url = relativeUrl,
            FileName = fileName,
            Size = file.Length
        };
    }
}
