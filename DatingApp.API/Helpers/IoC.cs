using DatingApp.API.Data;
using DatingApp.API.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DatingApp.API.Helpers
{
    public static class IoC
    {
        public static void RegisterServices(this IServiceCollection services)
        {
            services.AddScoped<IAuthRepository, AuthRepository>();
            services.AddScoped<IDatingRepository, DatingRepository>();
            services.AddScoped<ITokenService, JwtTokenService>();
            services.AddScoped<IPhotoUploader, CloudinaryPhotoUploader>();
        }
    }
}