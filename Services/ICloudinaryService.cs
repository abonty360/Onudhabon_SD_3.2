using Microsoft.AspNetCore.Http;

namespace Onudhabon_ISD.Services
{
    public class CloudinaryUploadResult
    {
        public bool Success { get; set; }
        public string? SecureUrl { get; set; }
        public string? ThumbnailUrl { get; set; }
        public string? PublicId { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Format { get; set; }
        public long Bytes { get; set; }
        public double SizeInMb => Bytes > 0 ? Math.Round((double)Bytes / (1024.0 * 1024.0), 2) : 0;
        public string FormattedSize => $"{SizeInMb:F2} MB";
    }

    public class CloudinaryResourceItem
    {
        public string PublicId { get; set; } = string.Empty;
        public string SecureUrl { get; set; } = string.Empty;
        public string? ThumbnailUrl { get; set; }
        public string? Format { get; set; }
        public long Bytes { get; set; }
        public DateTime CreatedAt { get; set; }
        public double SizeInMb => Bytes > 0 ? Math.Round((double)Bytes / (1024.0 * 1024.0), 2) : 0;
        public string FormattedSize => $"{SizeInMb:F2} MB";
        public string DisplayTitle => Path.GetFileNameWithoutExtension(PublicId).Replace("_", " ").Replace("-", " ");
    }

    public interface ICloudinaryService
    {
        /// <summary>
        /// Uploads a lecture video (mp4, mkv, webm) to 'onudhabon/lectures' with eager thumbnail transformation (300x200 crop fill jpg).
        /// </summary>
        Task<CloudinaryUploadResult> UploadLectureVideoAsync(IFormFile file);

        /// <summary>
        /// Uploads a material document (PDF, docx, etc.) to 'onudhabon/materials' with resource_type auto and size calculation.
        /// </summary>
        Task<CloudinaryUploadResult> UploadMaterialPdfAsync(IFormFile file);

        /// <summary>
        /// Uploads a profile picture to 'onudhabon/profile_pictures' with resource_type image.
        /// </summary>
        Task<CloudinaryUploadResult> UploadProfilePictureAsync(IFormFile file);

        /// <summary>
        /// Uploads a consent letter to 'onudhabon/consent_letters' with resource_type auto.
        /// </summary>
        Task<CloudinaryUploadResult> UploadConsentLetterAsync(IFormFile file);

        /// <summary>
        /// Uploads educational / certificate document during registration to 'onudhabon/education_doc'.
        /// </summary>
        Task<CloudinaryUploadResult> UploadEducationDocAsync(IFormFile file);

        /// <summary>
        /// Fetches all existing video assets from 'onudhabon/lectures' folder in Cloudinary.
        /// </summary>
        Task<List<CloudinaryResourceItem>> FetchCloudinaryLecturesAsync();

        /// <summary>
        /// Fetches all existing material/document assets from 'onudhabon/materials' folder in Cloudinary.
        /// </summary>
        Task<List<CloudinaryResourceItem>> FetchCloudinaryMaterialsAsync();

        /// <summary>
        /// Deletes an asset by public ID from Cloudinary.
        /// </summary>
        Task<bool> DeleteAsync(string publicId);

        /// <summary>
        /// Generates a transformed Cloudinary Image URL (resize, crop, optimize).
        /// </summary>
        string GetTransformedImageUrl(string? originalUrl, int? width = null, int? height = null, string crop = "fill", bool gravityFace = false, string? customTransformation = null);

        /// <summary>
        /// Generates a perfectly cropped face avatar thumbnail (e.g. 150x150 face-centered).
        /// </summary>
        string GetAvatarUrl(string? originalUrl, int size = 150);

        /// <summary>
        /// Generates an optimized video streaming URL.
        /// </summary>
        string GetOptimizedVideoUrl(string? originalUrl, int? maxWidth = null);

        /// <summary>
        /// Generates an eager/fallback 300x200 video thumbnail URL.
        /// </summary>
        string GetVideoThumbnailUrl(string? videoUrl, int width = 300, int height = 200);

        /// <summary>
        /// Generates a PDF first-page thumbnail image preview URL.
        /// </summary>
        string GetPdfThumbnailUrl(string? originalUrl, int width = 300, int page = 1);
    }
}