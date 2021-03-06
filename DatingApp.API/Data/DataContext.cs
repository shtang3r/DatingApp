using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DataContext : DbContext
    {
        public DataContext(DbContextOptions<DataContext> options) :base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Photo> Photos { get; set; }
        public DbSet<Like> Likes { get; set; }
        public DbSet<Message> Messages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<Like>().HasKey(entity=> new {entity.LikeeId, entity.LikerId});
            
            modelBuilder.Entity<Like>()
                .HasOne(u => u.Likee)
                .WithMany(u=>u.Likers)
                .HasForeignKey(u=>u.LikeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Like>()
                .HasOne(u => u.Liker)
                .WithMany(u=>u.Likees)
                .HasForeignKey(u=>u.LikerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m=>m.Sender)
                .WithMany(u=> u.MessagesSent)
                .HasForeignKey(m=>m.SenderId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Message>()
                .HasOne(m=>m.Recepient)
                .WithMany(u=> u.MessagesReceived)
                .HasForeignKey(m=>m.RecepientId)
                .OnDelete(DeleteBehavior.Restrict);           
        }
    }
}