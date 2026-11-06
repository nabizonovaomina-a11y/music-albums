 using Microsoft.EntityFrameworkCore;
using MusicAlbums.Api.Models;

namespace MusicAlbums.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Genre> Genres => Set<Genre>();
    public DbSet<Album> Albums => Set<Album>();
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<User>(e =>
        {
            e.Property(x => x.Username).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.Username).IsUnique();
            e.Property(x => x.PasswordHash).IsRequired();
        });

        b.Entity<Genre>(e =>
        {
            e.Property(x => x.Name).IsRequired().HasMaxLength(50);
            e.HasIndex(x => x.Name).IsUnique();
            e.HasData(
                new Genre { Id = 1, Name = "Rock" },
                new Genre { Id = 2, Name = "Pop" },
                new Genre { Id = 3, Name = "Hip-Hop" },
                new Genre { Id = 4, Name = "Jazz" },
                new Genre { Id = 5, Name = "Classical" });
        });

        b.Entity<Album>(e =>
        {
            e.Property(x => x.Title).IsRequired().HasMaxLength(200);
            e.Property(x => x.Artist).IsRequired().HasMaxLength(200);
            e.Property(x => x.CoverFileName).HasMaxLength(100);
            e.HasOne(x => x.Genre)
             .WithMany(g => g.Albums)
             .HasForeignKey(x => x.GenreId)
             .OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Owner)
             .WithMany(u => u.Albums)
             .HasForeignKey(x => x.OwnerId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}