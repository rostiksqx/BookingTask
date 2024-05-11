using Booking.Core.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Booking.Data;

public class BookingDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public BookingDbContext(DbContextOptions<BookingDbContext> options) : base(options)
    {
    }
    
    public DbSet<Housing> Housings { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasMany(u => u.Housings)
                .WithOne(h => h.User)
                .HasForeignKey(h => h.UserId);
        });

        modelBuilder.Entity<Housing>(entity =>
        {
            entity.HasKey(h => h.Id);
            
            entity.Property(h => h.Name).IsRequired();
            entity.Property(h => h.Description).HasMaxLength(250).IsRequired();
            entity.Property(h => h.Rooms).IsRequired();
            entity.Property(h => h.Address).IsRequired();
            entity.Property(h => h.IsBooked).IsRequired();
            
            entity.HasOne(h => h.User)
                .WithMany(u => u!.Housings)
                .HasForeignKey(h => h.UserId);
        });
    }
}