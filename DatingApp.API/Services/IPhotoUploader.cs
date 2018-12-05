using System.Threading.Tasks;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Services
{
    public interface IPhotoUploader
    {
         Task<PhotoForReturnDto> UploadPhotoAsync(int userId, PhotoForCreationDto photoForCreationDto);
         Task DeletePhotoAsync(Photo photo);
    }
}