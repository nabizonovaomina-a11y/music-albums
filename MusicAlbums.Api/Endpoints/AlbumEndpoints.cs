using Microsoft.EntityFrameworkCore;
using MusicAlbums.Api.Data;
using MusicAlbums.Api.Dtos;
using MusicAlbums.Api.Models;

namespace MusicAlbums.Api.Endpoints;

public static class AlbumEndpoints
{
    public static void MapAlbumEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/albums").WithTags("Albums");

        // GET list -> 200
        group.MapGet("/", async (AppDbContext db) =>
        {
            var albums = await db.Albums
                .AsNoTracking()
                .Include(a => a.Genre)
                .OrderBy(a => a.Id)
                .ToListAsync();
            return Results.Ok(albums.Select(ToDto));
        });

        // GET by id -> 200 / 404
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var album = await db.Albums
                .AsNoTracking()
                .Include(a => a.Genre)
                .FirstOrDefaultAsync(a => a.Id == id);
            return album is null ? Results.NotFound() : Results.Ok(ToDto(album));
        });

        // POST -> 201 / 400
        group.MapPost("/", async (AlbumRequest req, AppDbContext db) =>
        {
            if (!await db.Genres.AnyAsync(g => g.Id == req.GenreId))
                return GenreNotFound();

            var album = new Album
            {
                Title = req.Title,
                Artist = req.Artist,
                Year = req.Year,
                TrackCount = req.TrackCount,
                DurationMinutes = req.DurationMinutes,
                GenreId = req.GenreId
            };
            db.Albums.Add(album);
            await db.SaveChangesAsync();
            await db.Entry(album).Reference(a => a.Genre).LoadAsync();

            return Results.Created($"/api/albums/{album.Id}", ToDto(album));
        });

        // PUT -> 204 / 400 / 404
        group.MapPut("/{id:int}", async (int id, AlbumRequest req, AppDbContext db) =>
        {
            var album = await db.Albums.FindAsync(id);
            if (album is null) return Results.NotFound();

            if (!await db.Genres.AnyAsync(g => g.Id == req.GenreId))
                return GenreNotFound();

            album.Title = req.Title;
            album.Artist = req.Artist;
            album.Year = req.Year;
            album.TrackCount = req.TrackCount;
            album.DurationMinutes = req.DurationMinutes;
            album.GenreId = req.GenreId;
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        // DELETE -> 204 / 404
        group.MapDelete("/{id:int}", async (int id, AppDbContext db) =>
        {
            var album = await db.Albums.FindAsync(id);
            if (album is null) return Results.NotFound();

            db.Albums.Remove(album);
            await db.SaveChangesAsync();
            return Results.NoContent();
        });
    }

    private static AlbumDto ToDto(Album a) => new(
        a.Id, a.Title, a.Artist, a.Year, a.TrackCount, a.DurationMinutes,
        a.GenreId, a.Genre?.Name ?? "", a.CoverFileName);

    private static IResult GenreNotFound() =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["genreId"] = ["Bunday janr mavjud emas."]
        });
}