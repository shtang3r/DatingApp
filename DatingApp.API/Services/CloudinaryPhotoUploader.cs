using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Services
{
    public class CloudinaryPhotoUploader : IPhotoUploader
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;
        private readonly Cloudinary _cloudinary;
        private readonly IOptions<CloudinarySettings> _cloudinaryConfig;

        public CloudinaryPhotoUploader(IDatingRepository datingRepository,
                                       IOptions<CloudinarySettings> cloudinaryConfig,
                                       IMapper mapper)
        {
            _datingRepository = datingRepository;
            _mapper = mapper;
            _cloudinaryConfig = cloudinaryConfig;
            CloudinaryDotNet.Account acc =
                new CloudinaryDotNet.Account(_cloudinaryConfig.Value.CloudName,
                                             _cloudinaryConfig.Value.ApiKey,
                                             _cloudinaryConfig.Value.ApiSecret);

            _cloudinary = new Cloudinary(acc);
        }

        public async Task DeletePhotoAsync(Photo photo)
        {
            var deletionParams = new DeletionParams(photo.PublicId);
            await Task.Run(() => _cloudinary.Destroy(deletionParams));
        }

        public async Task<PhotoForReturnDto> UploadPhotoAsync(int userId, PhotoForCreationDto photoForCreationDto)
        {
            var userFromRepo = await _datingRepository.GetUserAsync(userId);
            var file = photoForCreationDto.File;
            var uploadResult = new ImageUploadResult();

            if (file.Length > 0)
            {
                using (var stream = file.OpenReadStream())
                {
                    var uploadParams = new ImageUploadParams
                    {
                        File = new FileDescription(file.Name, stream),
                        Transformation = new Transformation()
                            .Width(500)
                            .Height(500)
                            .Crop("fill")
                            .Gravity("face")
                    };

                    uploadResult = _cloudinary.Upload(uploadParams);
                }
            }

            photoForCreationDto.Url = uploadResult.Uri.ToString();
            photoForCreationDto.PublicId = uploadResult.PublicId;

            var photo = _mapper.Map<Photo>(photoForCreationDto);

            if (!userFromRepo.Photos.Any())
            {
                photo.IsMain = true;
            }

            userFromRepo.Photos.Add(photo);            

            if (await _datingRepository.SaveAllAsync())
            {
                var photoToReturn = _mapper.Map<PhotoForReturnDto>(photo);
                return photoToReturn;
                //return CreatedAtRoute("GetPhoto",new { id = photo.Id}, photoToReturn);
            }

            return null;
        }
    }
}