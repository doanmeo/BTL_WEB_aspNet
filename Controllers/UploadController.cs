using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BlogWebsite.Controllers
{
    [Authorize] // Yêu cầu đăng nhập để upload
    public class UploadController : Controller
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<UploadController> _logger;

        public UploadController(IWebHostEnvironment environment, ILogger<UploadController> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        [HttpPost]
        [IgnoreAntiforgeryToken] // Bỏ qua anti-forgery token cho API upload
        public async Task<IActionResult> UploadSingleImage(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    _logger.LogWarning("Upload attempt with null or empty file");
                    return BadRequest(new { error = "Vui lòng chọn file ảnh." });
                }

                // Kiểm tra kích thước file (tối đa 10MB)
                const long maxFileSize = 10 * 1024 * 1024; // 10MB
                if (file.Length > maxFileSize)
                {
                    _logger.LogWarning($"File too large: {file.Length} bytes");
                    return BadRequest(new { error = "File ảnh quá lớn. Kích thước tối đa là 10MB." });
                }

                // Kiểm tra loại file
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning($"Invalid file extension: {fileExtension}");
                    return BadRequest(new { error = "Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp)." });
                }

                // 1. Tạo thư mục lưu trữ nếu chưa có
                var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                    _logger.LogInformation($"Created uploads directory: {uploadsFolder}");
                }

                // 2. Tạo tên file độc nhất (loại bỏ ký tự đặc biệt trong tên file gốc)
                var sanitizedFileName = Path.GetFileNameWithoutExtension(file.FileName)
                    .Replace(" ", "_")
                    .Replace("(", "")
                    .Replace(")", "");
                var uniqueFileName = $"{Guid.NewGuid()}_{sanitizedFileName}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // 3. Lưu file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                _logger.LogInformation($"File uploaded successfully: {uniqueFileName}");

                // 4. Trả về đường dẫn ảnh (để JS chèn vào bài viết)
                var imageUrl = $"/uploads/{uniqueFileName}";
                return Ok(new { url = imageUrl });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file");
                return StatusCode(500, new { error = "Có lỗi xảy ra khi tải ảnh lên. Vui lòng thử lại." });
            }
        }
    }
}