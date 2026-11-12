 using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using MusicAlbums.Api.Data;
using MusicAlbums.Api.Dtos;
using MusicAlbums.Api.Filters;
using MusicAlbums.Api.Models;

namespace MusicAlbums.Api.Endpoints;

public static class AlbumEndpoints
{
    public static void MapAlbumEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/albums")
            .WithTags("Albums")
            .AddEndpointFilter<ValidationFilter<AlbumRequest>>();

        // GET list -> 200 (sahifalash, filtr, saralash)
        group.MapGet("/", async ([AsParameters] AlbumQuery q, AppDbContext db) =>
        {
            var page = Math.Max(q.Page ?? 1, 1);
            var pageSize = Math.Clamp(q.PageSize ?? 10, 1, 50);

            var query = db.Albums.AsNoTracking().Include(a => a.Genre).AsQueryable();

            // Filtrlar
            if (q.GenreId is not null)
                query = query.Where(a => a.GenreId == q.GenreId);
            if (!string.IsNullOrWhiteSpace(q.Artist))
                query = query.Where(a => EF.Functions.Like(a.Artist, $"%{q.Artist.Trim()}%"));
            if (q.Year is not null)
                query = query.Where(a => a.Year == q.Year);

            // Saralash: sort=year yoki sort=duration, dir=asc yoki desc
            var desc = string.Equals(q.Dir, "desc", StringComparison.OrdinalIgnoreCase);
            query = q.Sort?.ToLowerInvariant() switch
            {
                "year" => desc
                    ? query.OrderByDescending(a => a.Year).ThenBy(a => a.Id)
                    : query.OrderBy(a => a.Year).ThenBy(a => a.Id),
                "duration" => desc
                    ? query.OrderByDescending(a => a.DurationMinutes).ThenBy(a => a.Id)
                    : query.OrderBy(a => a.DurationMinutes).ThenBy(a => a.Id),
                _ => query.OrderBy(a => a.Id)
            };

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Results.Ok(new PagedResult<AlbumDto>(
                items.Select(ToDto).ToList(), page, pageSize, total));
        });

        // GET by id -> 200 / 404 (hamma uchun ochiq)
        group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
        {
            var album = await db.Albums
                .AsNoTracking()
                .Include(a => a.Genre)
                .FirstOrDefaultAsync(a => a.Id == id);
            return album is null ? Results.NotFound() : Results.Ok(ToDto(album));
        });

        // POST -> 201 / 400 / 401 (faqat kirgan foydalanuvchi)
        group.MapPost("/", async (AlbumRequest req, ClaimsPrincipal user, AppDbContext db) =>
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
                GenreId = req.GenreId,
                OwnerId = GetUserId(user)
            };
            db.Albums.Add(album);
            await db.SaveChangesAsync();
            await db.Entry(album).Reference(a => a.Genre).LoadAsync();

            return Results.Created($"/api/albums/{album.Id}", ToDto(album));
        }).RequireAuthorization();

        // PUT -> 204 / 400 / 401 / 403 / 404 (faqat muallif)
        group.MapPut("/{id:int}", async (int id, AlbumRequest req, ClaimsPrincipal user, AppDbContext db) =>
        {
            var album = await db.Albums.FindAsync(id);
            if (album is null) return Results.NotFound();
            if (album.OwnerId != GetUserId(user)) return NotOwner();

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
        }).RequireAuthorization();

        // DELETE -> 204 / 401 / 403 / 404 (faqat muallif)
        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal user, AppDbContext db) =>
        {
            var album = await db.Albums.FindAsync(id);
            if (album is null) return Results.NotFound();
            if (album.OwnerId != GetUserId(user)) return NotOwner();

            db.Albums.Remove(album);
            await db.SaveChangesAsync();
            return Results.NoContent();
        }).RequireAuthorization();
    }

    // Tokendagi foydalanuvchi id sini oladi
    private static int GetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? user.FindFirstValue("nameid")
                    ?? user.FindFirstValue("sub");
        return int.Parse(value!);
    }

    private static AlbumDto ToDto(Album a) => new(
        a.Id, a.Title, a.Artist, a.Year, a.TrackCount, a.DurationMinutes,
                a.GenreId, a.Genre?.Name ?? "",
        a.CoverFileName is null ? null : "/uploads/" + a.CoverFileName, a.OwnerId);

    private static IResult NotOwner() =>
        Results.Problem(
            statusCode: StatusCodes.Status403Forbidden,
            title: "Forbidden",
            detail: "Faqat albom muallifi o'zgartira yoki o'chira oladi.");

    private static IResult GenreNotFound() =>
        Results.ValidationProblem(new Dictionary<string, string[]>
        {
            ["genreId"] = ["Bunday janr mavjud emas."]
        });
}
