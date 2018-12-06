using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DatingApp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;

        public UsersController(IDatingRepository datingRepository, IMapper mapper)
        {
            this._datingRepository = datingRepository;
            this._mapper = mapper;
        }

        private int GetCurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);

        [HttpGet]
        public async Task<IActionResult> GetUsers([FromQuery] UserParams userParams)
        {
            var currentUserId = GetCurrentUserId();
            var userFromRepo = await _datingRepository.GetUserAsync(currentUserId);
            userParams.UserId = currentUserId;

            if (string.IsNullOrEmpty(userParams.Gender)) // smelly but whatever
            {
                userParams.Gender = "all";
            }

            var usersList = await _datingRepository.GetUsersAsync(userParams);
            var usersToReturn = _mapper.Map<IEnumerable<UserForListDto>>(usersList);
            Response.AddPaginationHeader(usersList.CurrentPage, usersList.PageSize, usersList.TotalCount, usersList.TotalPages);
            return Ok(usersToReturn);
        }

        [HttpGet("{id}", Name = "GetUser")]
        public async Task<IActionResult> GetUser(int id)
        {
            var current = Thread.CurrentPrincipal;
            var user = await _datingRepository.GetUserAsync(id);
            var userToReturn = _mapper.Map<UserForDetailedDto>(user);
            return Ok(userToReturn);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UserForUpdateDto user)
        {
            var userToUpdate = _mapper.Map<User>(user);
            if (id != GetCurrentUserId())
            {
                return Unauthorized();
            }

            var userFromRepository = await _datingRepository.GetUserAsync(id);
            _mapper.Map(user, userFromRepository);

            if (await _datingRepository.SaveAllAsync())
            {
                return NoContent();
            }

            throw new Exception($"Updating user {id} failed on save");
        }

        [HttpPost("{id}/like/{recepientId}")]
        public async Task<IActionResult> Like(int id, int recepientId)
        {
            if (id != GetCurrentUserId())
            {
                return Unauthorized();
            }
            var like = await _datingRepository.GetLikeAsync(id, recepientId);
            
            if (like != null)
            {
                return BadRequest("You already like this user");
            }

            if (await _datingRepository.GetUserAsync(recepientId) == null)
            {
                return NotFound("User you want to like doesn't exist");
            }

            like = new Like
            {
                LikerId = id,
                LikeeId = recepientId
            };

            _datingRepository.Add<Like>(like);

            if (await _datingRepository.SaveAllAsync())
            {
                return Ok();
            }

            return BadRequest();
        }
    }
}