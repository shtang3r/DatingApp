using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Claims;
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
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IDatingRepository _datingRepository;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository datingRepository,
                                  IMapper mapper)
        {
            _mapper = mapper;
            _datingRepository = datingRepository;
        }

        [HttpGet("{id}", Name = "GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (CurrentUserId() != userId)
            {
                return Unauthorized();
            }

            var messageFromRepo = await _datingRepository.GetMessageAsync(id);
            
            if (messageFromRepo != null)
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(messageFromRepo);
                return Ok(messageToReturn);
            }

            return BadRequest("Message is not found");
        }

        [HttpGet(Name = "GetMessages")]
        public async Task<IActionResult> GetMessages(int userId, [FromQuery] MessageParams messageParams)
        {
             if (CurrentUserId() != userId)
            {
                return Unauthorized();
            } 
            
            messageParams.UserId = userId;
            var messagesFromRepo = await _datingRepository.GetMessagesForUserAsync(messageParams);
            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPaginationHeader(messagesFromRepo.CurrentPage, 
                                         messagesFromRepo.PageSize,
                                         messagesFromRepo.TotalCount, 
                                         messagesFromRepo.TotalPages);
            return Ok(messages);
        }

        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, 
                                                       MessageForCreationDto messageForCreationDto)
        {
            if (CurrentUserId() != userId)
            {
                return Unauthorized();
            }

            messageForCreationDto.SenderId = userId;
            var recipient = await _datingRepository.GetUserAsync(messageForCreationDto.RecepientId);

            if (recipient == null)
            {
                return BadRequest("User not found");
            }

            var message = _mapper.Map<Message>(messageForCreationDto);
            _datingRepository.Add<Message>(message);

            if (await _datingRepository.SaveAllAsync())
            {
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                var senderPhotoUrl = (await _datingRepository.GetMainPhotoAsync(userId)).Url;
                messageToReturn.SenderPhotoUrl = senderPhotoUrl;
                return CreatedAtRoute("GetMessage", new { id = message.Id }, messageToReturn);
            }

            throw new Exception("Something went wrong during saving the message");
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (CurrentUserId() != userId)
            {
                return Unauthorized();
            }

            var messageFromRepo = await _datingRepository.GetMessageThreadAsync(userId, recipientId);
            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messageFromRepo);
            
            return Ok(messageThread);            
        }

        [HttpPost("{id}")]
        public async Task<IActionResult> DeleteMessage(int userId, int id)
        {
            if (CurrentUserId() != userId)
            {
                return Unauthorized();
            }

            var messageFromRepo = await _datingRepository.GetMessageAsync(id);

            if(messageFromRepo.SenderId == userId) 
            {
                messageFromRepo.SenderDeleted = true;
                
            }
            if (messageFromRepo.RecepientId == userId)
            {
                messageFromRepo.RecepientDeleted = true;               
            }            

            if (CanBeDeleted(messageFromRepo))
            {
                _datingRepository.Delete(messageFromRepo);
            }

            if(await _datingRepository.SaveAllAsync())
            {
                return NoContent();
            }

            throw new Exception("Error deleting the message");        
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId,int id)
        {
             if (CurrentUserId() != userId)
            {
                return Unauthorized();
            }

            var message = await _datingRepository.GetMessageAsync(id);
            if (message.RecepientId != userId)
            {
                return Unauthorized();
            }

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _datingRepository.SaveAllAsync();

            return NoContent();
        }

        private bool CanBeDeleted(Message messageFromRepo) 
        => messageFromRepo.RecepientDeleted && messageFromRepo.SenderDeleted;      

        private int CurrentUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
    }
}