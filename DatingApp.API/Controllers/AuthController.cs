using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using DatingApp.API.Services;
using System.Linq;
using AutoMapper;

namespace DatingApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthRepository _repository;
        private readonly IConfiguration _configuration;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AuthController(IAuthRepository repository,
                              IConfiguration configuration,
                              ITokenService tokenService,
                              IMapper mapper)
        {
            _configuration = configuration;
            _tokenService = tokenService;
            _mapper = mapper;
            _repository = repository;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserForRegisterDto userForRegisterDto)
        {
            userForRegisterDto.Username = userForRegisterDto.Username.ToLower();

            if (await _repository.UserExistsAsync(userForRegisterDto.Username))
            {
                return BadRequest("User with this name already exists");
            }

            var userToCreate = _mapper.Map<User>(userForRegisterDto);            
            var createdUser = await _repository.RegisterAsync(userToCreate, userForRegisterDto.Password);
            var userToReturn = _mapper.Map<UserForDetailedDto>(createdUser);

            return CreatedAtRoute("GetUser", new {controller = "Users", id = createdUser.Id}, userToReturn);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(UserForLoginDto userForLoginDto)
        {            
            var userFromRepo = await _repository.LoginAsync(userForLoginDto.Username.ToLower(), userForLoginDto.Password.ToLower());
            var userDto = _mapper.Map<UserForListDto>(userFromRepo);
            if (userFromRepo == null)
                return Unauthorized();

            var token = _tokenService.GetToken(userFromRepo);

            return Ok(new { token, user = userDto });
        }
    }
}