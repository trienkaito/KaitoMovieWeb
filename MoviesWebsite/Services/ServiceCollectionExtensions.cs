using MoviesWebsite.Models.Repository;
using MoviesWebsite.Models.Repository.Interface;

namespace MoviesWebsite.Services
{
    public static class ServiceCollectionExtensions
    {
        public static void AddCloudinary(this IServiceCollection services, IConfiguration configuration)
        {
            var cloudinarySection = configuration.GetSection("Cloudinary");
            var cloudinaryConfig = new CloudinaryDotNet.Account(
                cloudinarySection["CloudName"],
                cloudinarySection["ApiKey"],
                cloudinarySection["ApiSecret"]
            );

            if (string.IsNullOrEmpty(cloudinaryConfig.Cloud) ||
                string.IsNullOrEmpty(cloudinaryConfig.ApiKey) ||
                string.IsNullOrEmpty(cloudinaryConfig.ApiSecret))
            {
                throw new ArgumentException("Cloudinary configuration is missing or incomplete");
            }

            var cloudinary = new CloudinaryDotNet.Cloudinary(cloudinaryConfig);
            services.AddSingleton(cloudinary);
            services.AddScoped<ICloudinaryService, CloudinaryService>();
        }
    }
}
