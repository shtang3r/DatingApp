using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using DatingApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace DatingApp.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users/{userId}/photos")]
    public class PhotosController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;        
        private readonly IPhotoUploader _photoUploader;        

        public PhotosController(IDatingRepository datingRepository,
                                IMapper mapper,                                
                                IPhotoUploader photoUploader)
        {            
            _photoUploader = photoUploader;
            _mapper = mapper;
            _datingRepository = datingRepository;
        }

        [HttpGet("{id}", Name = "GetPhoto")]
        public async Task<IActionResult> GetPhoto(int id)
        {
            var photoFromRepo = await _datingRepository.GetPhotoAsync(id);

            var photo = _mapper.Map<PhotoForReturnDto>(photoFromRepo);

            return Ok(photo);
        }

        [HttpPost]
        public async Task<IActionResult> AddPhotoForUser(int userId, [FromForm]PhotoForCreationDto photoForCreationDto)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var photoToReturn = await _photoUploader.UploadPhotoAsync(userId, photoForCreationDto);
           
            if (photoToReturn != null)
            {
                return CreatedAtRoute("GetPhoto", new { id = photoToReturn.Id }, photoToReturn);
            }            

            return BadRequest("Could not add the photo");
        }

        [HttpPut("{id}/setMain")]
        public async Task<IActionResult> SetMain(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await _datingRepository.GetUserAsync(userId);
            if (!user.Photos.Any(p=> p.Id == id))
            {
                return NotFound($"Photo with id={id} is not found");
            } 
            
            var photoFromRepo = await _datingRepository.GetPhotoAsync(id);
            if (photoFromRepo.IsMain)
            {
                return BadRequest("This is already the main photo");
            }

            var currentMainPhoto = await _datingRepository.GetMainPhotoAsync(userId);
            currentMainPhoto.IsMain = false;
            photoFromRepo.IsMain = true;
            
            if (await _datingRepository.SaveAllAsync())
            {
                return NoContent();
            }

            return BadRequest("Could not set photo to main");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePhoto(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
            {
                return Unauthorized();
            }

            var user = await _datingRepository.GetUserAsync(userId);
            if (!user.Photos.Any(p=> p.Id == id))
            {
                return NotFound($"Photo with id={id} is not found");
            } 
            
            var photoFromRepo = await _datingRepository.GetPhotoAsync(id);
            if (photoFromRepo.IsMain)
            {
                return BadRequest("Can't delete Main photo");
            }

            if (photoFromRepo.PublicId != null)
            {                
                await _photoUploader.DeletePhotoAsync(photoFromRepo);
            }           
            
            _datingRepository.Delete(photoFromRepo);
            
            if (await _datingRepository.SaveAllAsync())
            {
                return Ok();
            }
            
            return BadRequest("Failed to delete the photo");
        }
    }
}