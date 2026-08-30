using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Onudhabon_ISD.Services
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryService> _logger;

        public const string FOLDER_LECTURES = "onudhabon/lectures";
        public const string FOLDER_MATERIALS = "onudhabon/materials";
        public const string FOLDER_PROFILES = "onudhabon/profile_pictures";
        public const string FOLDER_CONSENT = "onudhabon/consent_letters";
        public const string FOLDER_EDUCATION = "onudhabon/education_doc";

        public CloudinaryService(ILogger<CloudinaryService> logger)
        {
            _logger = logger;

            var cloudName = Environment.GetEnvironmentVariable("CLOUDINARY_CLOUD_NAME")
                            ?? Environment.GetEnvironmentVariable("CloudName")
                            ?? string.Empty;

            var apiKey = Environment.GetEnvironmentVariable("CLOUDINARY_API_KEY")
                         ?? Environment.GetEnvironmentVariable("ApiKey")
                         ?? string.Empty;

            var apiSecret = Environment.GetEnvironmentVariable("CLOUDINARY_API_SECRET")
                            ?? Environment.GetEnvironmentVariable("ApiSecret")
                            ?? string.Empty;

            if (string.IsNullOrWhiteSpace(cloudName) ||
                string.IsNullOrWhiteSpace(apiKey) ||
                string.IsNullOrWhiteSpace(apiSecret))
            {
                _logger.LogWarning("Cloudinary credentials missing in .env (CloudName, ApiKey, ApiSecret). Please configure them.");
            }

            var account = new Account(cloudName, apiKey, apiSecret);
            _cloudinary = new Cloudinary(account);
            _cloudinary.Api.Secure = true;
        }

        public async Task<CloudinaryUploadResult> UploadLectureVideoAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new CloudinaryUploadResult { Success = false, ErrorMessage = "Video file is required." };
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var publicId = Path.GetFileNameWithoutExtension(file.FileName);

                var uploadParams = new VideoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = FOLDER_LECTURES,
                    PublicId = publicId,
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true,
                    EagerTransforms = new List<Transformation>
                    {
                        new Transformation().Width(300).Height(200).Crop("fill").FetchFormat("jpg")
                    },
                    EagerAsync = false
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary video upload error: {Message}", result.Error.Message);
                    return new CloudinaryUploadResult { Success = false, ErrorMessage = result.Error.Message };
                }

                var secureUrl = result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? string.Empty;
                var eagerThumbnail = GetVideoThumbnailUrl(secureUrl, 300, 200);

                return new CloudinaryUploadResult
                {
                    Success = true,
                    SecureUrl = secureUrl,
                    ThumbnailUrl = eagerThumbnail,
                    PublicId = result.PublicId,
                    Format = result.Format,
                    Bytes = result.Bytes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload video to Cloudinary ({FileName})", file.FileName);
                return new CloudinaryUploadResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<CloudinaryUploadResult> UploadMaterialPdfAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new CloudinaryUploadResult { Success = false, ErrorMessage = "Material file is required." };
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var publicId = Path.GetFileNameWithoutExtension(file.FileName);

                var uploadParams = new AutoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = FOLDER_MATERIALS,
                    PublicId = publicId,
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary material PDF upload error: {Message}", result.Error.Message);
                    return new CloudinaryUploadResult { Success = false, ErrorMessage = result.Error.Message };
                }

                var secureUrl = result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? string.Empty;

                return new CloudinaryUploadResult
                {
                    Success = true,
                    SecureUrl = secureUrl,
                    ThumbnailUrl = GetPdfThumbnailUrl(secureUrl, 300, 1),
                    PublicId = result.PublicId,
                    Format = result.Format,
                    Bytes = result.Bytes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload material to Cloudinary ({FileName})", file.FileName);
                return new CloudinaryUploadResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<CloudinaryUploadResult> UploadProfilePictureAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new CloudinaryUploadResult { Success = false, ErrorMessage = "Profile image file is required." };
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var publicId = Path.GetFileNameWithoutExtension(file.FileName);

                var uploadParams = new ImageUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = FOLDER_PROFILES,
                    PublicId = publicId,
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary profile upload error: {Message}", result.Error.Message);
                    return new CloudinaryUploadResult { Success = false, ErrorMessage = result.Error.Message };
                }

                var secureUrl = result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? string.Empty;

                return new CloudinaryUploadResult
                {
                    Success = true,
                    SecureUrl = secureUrl,
                    PublicId = result.PublicId,
                    Format = result.Format,
                    Bytes = result.Bytes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload profile image to Cloudinary ({FileName})", file.FileName);
                return new CloudinaryUploadResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<CloudinaryUploadResult> UploadConsentLetterAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new CloudinaryUploadResult { Success = false, ErrorMessage = "Consent file is required." };
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var publicId = Path.GetFileNameWithoutExtension(file.FileName);

                var uploadParams = new AutoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = FOLDER_CONSENT,
                    PublicId = publicId,
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary consent letter upload error: {Message}", result.Error.Message);
                    return new CloudinaryUploadResult { Success = false, ErrorMessage = result.Error.Message };
                }

                return new CloudinaryUploadResult
                {
                    Success = true,
                    SecureUrl = result.SecureUrl?.ToString() ?? result.Url?.ToString(),
                    PublicId = result.PublicId,
                    Bytes = result.Bytes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload consent letter to Cloudinary ({FileName})", file.FileName);
                return new CloudinaryUploadResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<CloudinaryUploadResult> UploadEducationDocAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new CloudinaryUploadResult { Success = false, ErrorMessage = "Education document file is required." };
            }

            try
            {
                await using var stream = file.OpenReadStream();
                var publicId = Path.GetFileNameWithoutExtension(file.FileName);

                var uploadParams = new AutoUploadParams
                {
                    File = new FileDescription(file.FileName, stream),
                    Folder = FOLDER_EDUCATION,
                    PublicId = publicId,
                    UseFilename = true,
                    UniqueFilename = false,
                    Overwrite = true
                };

                var result = await _cloudinary.UploadAsync(uploadParams);

                if (result.Error != null)
                {
                    _logger.LogError("Cloudinary education doc upload error: {Message}", result.Error.Message);
                    return new CloudinaryUploadResult { Success = false, ErrorMessage = result.Error.Message };
                }

                var secureUrl = result.SecureUrl?.ToString() ?? result.Url?.ToString() ?? string.Empty;

                return new CloudinaryUploadResult
                {
                    Success = true,
                    SecureUrl = secureUrl,
                    ThumbnailUrl = GetPdfThumbnailUrl(secureUrl, 300, 1),
                    PublicId = result.PublicId,
                    Bytes = result.Bytes
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload education document to Cloudinary ({FileName})", file.FileName);
                return new CloudinaryUploadResult { Success = false, ErrorMessage = ex.Message };
            }
        }

        public async Task<List<CloudinaryResourceItem>> FetchCloudinaryLecturesAsync()
        {
            var list = new List<CloudinaryResourceItem>();
            if (_cloudinary == null) return list;

            try
            {
                var searchResult = await _cloudinary.Search()
                    .Expression($"folder:{FOLDER_LECTURES}*")
                    .MaxResults(500)
                    .ExecuteAsync();

                if (searchResult.Resources != null)
                {
                    foreach (var res in searchResult.Resources)
                    {
                        var secUrl = res.SecureUrl ?? res.Url ?? string.Empty;
                        list.Add(new CloudinaryResourceItem
                        {
                            PublicId = res.PublicId,
                            SecureUrl = secUrl,
                            ThumbnailUrl = GetVideoThumbnailUrl(secUrl, 300, 200),
                            Format = res.Format,
                            Bytes = res.Bytes,
                            CreatedAt = DateTime.TryParse(res.CreatedAt, out var dt) ? dt : DateTime.UtcNow
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching lectures from Cloudinary folder {Folder}", FOLDER_LECTURES);
            }

            return list;
        }

        public async Task<List<CloudinaryResourceItem>> FetchCloudinaryMaterialsAsync()
        {
            var list = new List<CloudinaryResourceItem>();
            if (_cloudinary == null) return list;

            try
            {
                var searchResult = await _cloudinary.Search()
                    .Expression($"folder:{FOLDER_MATERIALS}*")
                    .MaxResults(500)
                    .ExecuteAsync();

                if (searchResult.Resources != null)
                {
                    foreach (var res in searchResult.Resources)
                    {
                        var secUrl = res.SecureUrl ?? res.Url ?? string.Empty;
                        list.Add(new CloudinaryResourceItem
                        {
                            PublicId = res.PublicId,
                            SecureUrl = secUrl,
                            ThumbnailUrl = GetPdfThumbnailUrl(secUrl, 300, 1),
                            Format = res.Format,
                            Bytes = res.Bytes,
                            CreatedAt = DateTime.TryParse(res.CreatedAt, out var dt) ? dt : DateTime.UtcNow
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching materials from Cloudinary folder {Folder}", FOLDER_MATERIALS);
            }

            return list;
        }

        public async Task<bool> DeleteAsync(string publicId)
        {
            if (string.IsNullOrWhiteSpace(publicId)) return false;

            try
            {
                var delParams = new DeletionParams(publicId);
                var result = await _cloudinary.DestroyAsync(delParams);
                return result.Result == "ok";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to delete Cloudinary asset {PublicId}", publicId);
                return false;
            }
        }

        public string GetTransformedImageUrl(
            string? originalUrl,
            int? width = null,
            int? height = null,
            string crop = "fill",
            bool gravityFace = false,
            string? customTransformation = null)
        {
            if (string.IsNullOrWhiteSpace(originalUrl)) return string.Empty;

            if (!originalUrl.Contains("res.cloudinary.com") || !originalUrl.Contains("/image/upload/"))
            {
                return originalUrl;
            }

            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(customTransformation))
            {
                parts.Add(customTransformation);
            }
            else
            {
                if (width.HasValue) parts.Add($"w_{width.Value}");
                if (height.HasValue) parts.Add($"h_{height.Value}");
                if (!string.IsNullOrWhiteSpace(crop) && (width.HasValue || height.HasValue)) parts.Add($"c_{crop}");
                if (gravityFace) parts.Add("g_face");
                parts.Add("f_auto");
                parts.Add("q_auto");
            }

            var transformString = string.Join(",", parts);
            if (string.IsNullOrWhiteSpace(transformString)) return originalUrl;

            return originalUrl.Replace("/image/upload/", $"/image/upload/{transformString}/");
        }

        public string GetAvatarUrl(string? originalUrl, int size = 150)
        {
            return GetTransformedImageUrl(
                originalUrl,
                width: size,
                height: size,
                crop: "thumb",
                gravityFace: true);
        }

        public string GetOptimizedVideoUrl(string? originalUrl, int? maxWidth = null)
        {
            if (string.IsNullOrWhiteSpace(originalUrl)) return string.Empty;

            if (!originalUrl.Contains("res.cloudinary.com") || !originalUrl.Contains("/video/upload/"))
            {
                return originalUrl;
            }

            var parts = new List<string> { "f_auto", "q_auto", "vc_auto" };
            if (maxWidth.HasValue)
            {
                parts.Add($"w_{maxWidth.Value}");
                parts.Add("c_limit");
            }

            var transformString = string.Join(",", parts);
            return originalUrl.Replace("/video/upload/", $"/video/upload/{transformString}/");
        }

        public string GetVideoThumbnailUrl(string? videoUrl, int width = 480, int height = 270)
        {
            if (string.IsNullOrWhiteSpace(videoUrl)) return string.Empty;

            if (!videoUrl.Contains("res.cloudinary.com"))
            {
                return videoUrl;
            }

            if (videoUrl.Contains("/video/upload/"))
            {
                var uploadIndex = videoUrl.IndexOf("/video/upload/", StringComparison.OrdinalIgnoreCase);
                var prefix = videoUrl.Substring(0, uploadIndex + "/video/upload/".Length);
                var rest = videoUrl.Substring(uploadIndex + "/video/upload/".Length);

                // Strip existing transformation parameters if present
                if (System.Text.RegularExpressions.Regex.IsMatch(rest, @"^(?:(?:[a-zA-Z0-9_-]+(?:_[a-zA-Z0-9_-]+)*,?)+/)?v\d+/"))
                {
                    rest = System.Text.RegularExpressions.Regex.Replace(rest, @"^(?:[a-zA-Z0-9_-]+(?:_[a-zA-Z0-9_-]+)*,?)+/(v\d+/)", "$1");
                }
                else if (System.Text.RegularExpressions.Regex.IsMatch(rest, @"^[a-zA-Z0-9_,]+/(?!v\d+)"))
                {
                    rest = System.Text.RegularExpressions.Regex.Replace(rest, @"^[a-zA-Z0-9_,]+/", "");
                }

                // Change extension to .jpg for video thumbnail delivery
                rest = System.Text.RegularExpressions.Regex.Replace(rest, @"\.(mp4|mkv|webm|mov|avi|flv|wmv|m4v|jpg|jpeg|png)(\?.*)?$", ".jpg$2", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (!rest.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase) && !rest.Contains(".jpg?"))
                {
                    rest += ".jpg";
                }

                var transform = $"so_0,w_{width},h_{height},c_fill,f_jpg";
                return $"{prefix}{transform}/{rest}";
            }

            return videoUrl;
        }

        public string GetPdfThumbnailUrl(string? originalUrl, int width = 300, int page = 1)
        {
            if (string.IsNullOrWhiteSpace(originalUrl)) return string.Empty;

            if (!originalUrl.Contains("res.cloudinary.com"))
            {
                return originalUrl;
            }

            var transform = $"pg_{page},w_{width},c_limit,f_auto,q_auto";

            if (originalUrl.Contains("/image/upload/"))
            {
                var replaced = originalUrl.Replace("/image/upload/", $"/image/upload/{transform}/");
                return Path.ChangeExtension(replaced, ".jpg");
            }

            if (originalUrl.Contains("/raw/upload/"))
            {
                var replaced = originalUrl.Replace("/raw/upload/", $"/image/upload/{transform}/");
                return Path.ChangeExtension(replaced, ".jpg");
            }

            if (originalUrl.Contains("/auto/upload/"))
            {
                var replaced = originalUrl.Replace("/auto/upload/", $"/image/upload/{transform}/");
                return Path.ChangeExtension(replaced, ".jpg");
            }

            return originalUrl;
        }
    }
}
