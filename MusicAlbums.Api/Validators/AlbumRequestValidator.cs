using FluentValidation;
using MusicAlbums.Api.Dtos;

namespace MusicAlbums.Api.Validators;

public class AlbumRequestValidator : AbstractValidator<AlbumRequest>
{
    public AlbumRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Artist).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Year).InclusiveBetween(1900, DateTime.UtcNow.Year + 1);
        RuleFor(x => x.TrackCount).InclusiveBetween(1, 100);
        RuleFor(x => x.DurationMinutes).GreaterThan(0);
        RuleFor(x => x.GenreId).GreaterThan(0);

        // Maxsus qoida: davomiylik (daqiqa) treklar sonidan kam bo'lmasin
        RuleFor(x => x.DurationMinutes)
            .GreaterThanOrEqualTo(x => x.TrackCount)
            .WithMessage("Davomiylik (daqiqa) treklar sonidan kam bo'lmasligi kerak.");
    }
}