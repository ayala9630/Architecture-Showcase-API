using ChineseSaleApi.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[Admin]
public class FilesController : ControllerBase
{
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<FilesController> _logger;

    // הגבלות מדיניות
    private static readonly string[] AllowedExtensions = new[] { ".png", ".jpg", ".jpeg", ".gif", ".svg", ".webp" };
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; 

    public FilesController(IWebHostEnvironment env, ILogger<FilesController> logger)
    {
        _env = env;
        _logger = logger;
    }

    [HttpPost("upload")]
    [RequestSizeLimit(10 * 1024 * 1024)] // הגבלה מקומית נוספת (10MB)
    public async Task<IActionResult> Upload(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return NotFound();
            }

            if (file.Length > MaxFileSizeBytes)
            {
                return BadRequest(new { error = $"File too large. Max allowed is {MaxFileSizeBytes} bytes." });
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !AllowedExtensions.Contains(ext))
            {
                return BadRequest(new { error = "File type not allowed." });
            }

            // תקין: יצירת תיקיית יעד בתוך wwwroot/images
            var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "images");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // שם ייחודי ובטוח
            var safeFileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, safeFileName);

            // שמירת הקובץ
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // הרכב relative + full URL
            var request = HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}";
            var relativeUrl = $"{safeFileName}";
            var fullUrl = $"{relativeUrl}";

            _logger.LogInformation("File uploaded: {FileName}, Size: {Size} bytes, From: {Ip}",
                safeFileName, file.Length, HttpContext.Connection.RemoteIpAddress);

            return Ok(new { fileName = safeFileName, url = fullUrl, relative = relativeUrl });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload file");
            return StatusCode(500, new { error = "An unexpected error occurred while uploading the file." });
        }
    }
}