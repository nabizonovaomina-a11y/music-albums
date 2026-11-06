 using Microsoft.EntityFrameworkCore;
using MusicAlbums.Api.Data;
using MusicAlbums.Api.Endpoints;
using Scalar.AspNetCore;
using FluentValidation;
using MusicAlbums.Api.Validators;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddValidatorsFromAssemblyContaining<AlbumRequestValidator>();
builder.Services.AddDbContext<AppDbContext>(o =>
    o.UseSqlite(builder.Configuration.GetConnectionString("Default")));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();   // /scalar/v1
}

app.UseHttpsRedirection();
app.MapAlbumEndpoints();

app.Run();