using System.Security.Claims;
using MusicAlbums.Api.Data;

namespace MusicAlbums.Api.Endpoints;

public static class CoverEndpoints
{
    private const long MaxBytes = 5 * 1024 * 1024;
    private static readonly string[] AllowedExt = [".jpg", ".jpeg", ".png", ".webp"];

    public static void MapCoverEndpoints(this WebApplication app)
    {
        // POST -> 200 / 400 / 401 / 403 / 404 (faqat muallif)
        app.MapPost("/api/albums/{id:int}/cover", async (
            int id, IFormFile file, ClaimsPrincipal user,
            AppDbContext db, IWebHostEnvironment env) =>
        {
            var album = await db.Albums.FindAsync(id);
            if (album is null) return Results.NotFound();

            var userId = int.Parse(user.FindFirstValue(ClaimTypes.NameIdentifier)!);
            if (album.OwnerId != userId)
                return Results.Problem(
                    statusCode: StatusCodes.Status403Forbidden,
                    title: "Forbidden",
                    detail: "Faqat albom muallifi muqova yuklay oladi.");

            if (file.Length == 0)
                return Bad("Fayl bo'sh.");
            if (file.Length > MaxBytes)
                return Bad("Fayl hajmi 5 MB dan oshmasligi kerak.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedExt.Contains(ext))
                return Bad("Faqat jpg, png yoki webp fayllar ruxsat etiladi.");

            // Fayl ichidagi haqiqiy turini tekshirish
            var header = new byte[12];
            await using (var s = file.OpenReadStream())
                await s.ReadExactlyAsync(header, 0, (int)Math.Min(12, file.Length));
            if (!LooksLikeImage(header))
                return Bad("Fayl rasm emas yoki buzilgan.");

            var dir = Path.Combine(env.ContentRootPath, "uploads");
            Directory.CreateDirectory(dir);

            var fileName = $"{Guid.NewGuid()}{ext}";
            await using (var fs = File.Create(Path.Combine(dir, fileName)))
                await file.CopyToAsync(fs);

            // Eski muqovani o'chirish
            if (album.CoverFileName is not null)
            {
                var old = Path.Combine(dir, album.CoverFileName);
                if (File.Exists(old)) File.Delete(old);
            }

            album.CoverFileName = fileName;
            await db.SaveChangesAsync();

            return Results.Ok(new { coverUrl = $"/uploads/{fileName}" });
        })
        .RequireAuthorization()
        .DisableAntiforgery()
        .WithTags("Albums");
    }

    private static IResult Bad(string message) =>
        Results.ValidationProblem(new Dictionary<string, string[]> { ["file"] = [message] });

    private static bool LooksLikeImage(byte[] h)
    {
        var jpg = h[0] == 0xFF && h[1] == 0xD8 && h[2] == 0xFF;
        var png = h[0] == 0x89 && h[1] == 0x50 && h[2] == 0x4E && h[3] == 0x47;
        var webp = h[0] == 'R' && h[1] == 'I' && h[2] == 'F' && h[3] == 'F'
                   && h[8] == 'W' && h[9] == 'E' && h[10] == 'B' && h[11] == 'P';
        return jpg || png || webp;
    }
}