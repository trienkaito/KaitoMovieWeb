using MoviesWebsite.Models.Movie;

namespace MoviesWebsite.Models.Repository.Interface
{
    public interface ICloudinaryService
    {
        Task<UploadResultDto> UploadToCloudinaryAsync(IFormFile file);
    }
}
