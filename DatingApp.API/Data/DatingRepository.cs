using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _dbContext;

        public DatingRepository(DataContext dbContext)
        {
            this._dbContext = dbContext;
        }

        public void Add<T>(T entity) where T : class
        {
            _dbContext.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _dbContext.Remove(entity);
        }

        public async Task<Like> GetLikeAsync(int userId, int recipientId)
        {
            return await _dbContext.Likes
                         .FirstOrDefaultAsync(u=>u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Photo> GetMainPhotoAsync(int userId)
        {
            return await _dbContext.Photos.Where(p => p.UserId == userId).FirstOrDefaultAsync(p=> p.IsMain);
        }

        public async Task<Photo> GetPhotoAsync(int id)
        {
            var photo = await _dbContext.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<User> GetUserAsync(int id)
        {
            return await _dbContext.Users.Include(u=>u.Photos).FirstOrDefaultAsync(user => user.Id == id);
        }

        public async Task<PagedList<User>> GetUsersAsync(UserParams userParams)
        {
            var users =  _dbContext.Users.Include(u=>u.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId);

            if (userParams.Gender != "all")
            { 
                users = users.Where(u=> u.Gender == userParams.Gender);
            }

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u=> userLikers.Contains(u.Id));
            }
            if (userParams.Likees) 
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u=> userLikees.Contains(u.Id));
            }

            // if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            // {
            var minDob = DateTime.Today.AddYears(-userParams.MaxAge);
            var maxDob = DateTime.Today.AddYears(-userParams.MinAge);

            users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
            // }
            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.CreatedOn);
                        break;
                    default: // userParams.OrderBy == "lastActive"
                        users = users.OrderByDescending( u => u.LastActive);
                        break;
                }
            }
            
            

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int userId, bool likers)
        {
            var user = await _dbContext.Users
                .Include(x => x.Likers)
                .Include(x => x.Likees)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (likers)
            {
                return user.Likers.Where(u => u.LikeeId == userId).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == userId).Select(i => i.LikeeId);
            }
        }

        public async Task<bool> SaveAllAsync()
        {
            return await _dbContext.SaveChangesAsync() > 0;
        }

        public async Task<Message> GetMessageAsync(int id)
        {
            return await _dbContext.Messages
                .Include(u=>u.Sender)
                .ThenInclude(p=>p.Photos)
                .Include(u=>u.Recepient)
                .ThenInclude(p=>p.Photos)
                .FirstOrDefaultAsync(m=>m.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUserAsync(MessageParams messageParams)
        {
            var messages = _dbContext.Messages
                .Include(u=>u.Sender).ThenInclude(p=>p.Photos)
                .Include(u=>u.Recepient).ThenInclude(p=>p.Photos)
                .AsQueryable();

            switch (messageParams.MessageContainer.ToLower())
            {
                case "inbox":
                    messages = messages.Where(u=> u.RecepientId == messageParams.UserId && !u.RecepientDeleted);
                    break;
                case "outbox":
                    messages = messages.Where(u=> u.SenderId == messageParams.UserId && !u.SenderDeleted);    
                    break;
                default:
                    messages = messages.Where(u => u.RecepientId == messageParams.UserId 
                                                && !u.RecepientDeleted && !u.IsRead);
                    break;
            }

            messages = messages.OrderByDescending(m=> m.MessageSent);
            return await PagedList<Message>.CreateAsync(messages,messageParams.PageNumber,messageParams.PageSize);    
        }

        public async Task<IEnumerable<Message>> GetMessageThreadAsync(int userId, int recipientId)
        {
            return await _dbContext.Messages
                .Include(u => u.Sender).ThenInclude(p => p.Photos)
                .Include(u => u.Recepient).ThenInclude(p => p.Photos)
                .Where(u => u.RecepientId == userId && !u.RecepientDeleted && u.SenderId == recipientId
                        || u.RecepientId == recipientId && u.SenderId == userId && !u.SenderDeleted)
                .OrderByDescending(m => m.MessageSent)
                .ToListAsync();
        }
    }
}