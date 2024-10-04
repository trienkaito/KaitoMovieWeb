using CloudinaryDotNet.Actions;
using CloudinaryDotNet;
using MoviesWebsite.Models.Movie;
using MoviesWebsite.Models.Repository.Interface;
using Xabe.FFmpeg;
using System.Diagnostics;

namespace MoviesWebsite.Models.Repository
{
    public class CloudinaryService : ICloudinaryService
    {
        private readonly CloudinaryDotNet.Cloudinary _cloudinary;

        public CloudinaryService(Cloudinary cloudinary)
        {
            _cloudinary = cloudinary;
        }

        public async Task<UploadResultDto> UploadToCloudinaryAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return new UploadResultDto { Url = string.Empty };
            }

            var fileType = file.ContentType.Split('/')[0];
            UploadResultDto result = new UploadResultDto();

            if (fileType.Equals("image"))
            {
                var uploadParams = new ImageUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream()),
                    EagerTransforms = new List<Transformation> {
                        new Transformation().Width(250).Height(250).Crop("fill").Gravity("auto").FetchFormat("jpg")
                    }
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult == null || uploadResult.SecureUrl == null)
                {
                    throw new Exception($"Upload to Cloudinary failed: {uploadResult.Error.Message}");
                }

                result.Url = uploadResult.SecureUrl.AbsoluteUri;
            }
            else if (fileType.Equals("video"))
            {
                var uploadParams = new VideoUploadParams()
                {
                    File = new FileDescription(file.FileName, file.OpenReadStream())
                };

                var uploadResult = await _cloudinary.UploadAsync(uploadParams);

                if (uploadResult == null || uploadResult.SecureUrl == null)
                {
                    throw new Exception($"Upload to Cloudinary failed: {uploadResult.Error.Message}");
                }


                // Tạo URL phát video HLS từ Cloudinary mà không có hậu tố
                // var hlsUrl = $"{uploadResult.SecureUrl.ToString().Replace(".mp4", ".m3u8")}/fl_attachment:streaming.m3u8";
                var hlsUrl = $"{uploadResult.SecureUrl.ToString().Replace(".mp4", ".m3u8")}";

                result.Url = hlsUrl; // URL của file HLS (.m3u8)
                result.ThumbnailUrl = uploadResult.SecureUrl.AbsoluteUri; // Thumbnail của video
            }

            return result;
        }
    }
}
