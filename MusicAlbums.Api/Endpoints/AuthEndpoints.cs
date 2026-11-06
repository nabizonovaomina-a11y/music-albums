using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MusicAlbums.Api.Data;
using MusicAlbums.Api.Dtos;
using MusicAlbums.Api.Filters;
using MusicAlbums.Api.Models;
using MusicAlbums.Api.Services;

namespace MusicAlbums.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth").WithTags("Auth");

        // Ro'yxatdan o'tish -> 201 / 400
        group.MapPost("/register", async (RegisterRequest req, AppDbContext db,
            IPasswordHasher<User> hasher, TokenService tokens) =>
        {
            var username = req.Username.Trim();
            if (await db.Users.AnyAsync(u => u.Username == username))
                return Results.ValidationProblem(new Dictionary<string, string[]>
                {
                    ["username"] = ["Bu nom band."]
                });

            var user = new User { Username = username };
            user.PasswordHash = hasher.HashPassword(user, req.Password);
            db.Users.Add(user);
            await db.SaveChangesAsync();

            return Results.Created("/api/auth/login",
                new AuthResponse(tokens.Create(user), user.Username));
        }).AddEndpointFilter<ValidationFilter<RegisterRequest>>();

        // Kirish -> 200 / 400 / 401
        group.MapPost("/login", async (LoginRequest req, AppDbContext db,
            IPasswordHasher<User> hasher, TokenService tokens) =>
        {
            var username = req.Username.Trim();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (user is null) return Results.Unauthorized();

            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, req.Password);
            if (result == PasswordVerificationResult.Failed) return Results.Unauthorized();

            return Results.Ok(new AuthResponse(tokens.Create(user), user.Username));
        }).AddEndpointFilter<ValidationFilter<LoginRequest>>();
    }
}