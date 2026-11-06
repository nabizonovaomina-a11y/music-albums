namespace MusicAlbums.Api.Dtos;

public record GenreDto(int Id, string Name);

public record AlbumDto(
    int Id, string Title, string Artist, int Year,
    int TrackCount, int DurationMinutes,
    int GenreId, string GenreName, string? CoverUrl);

// POST va PUT uchun
public record AlbumRequest(
    string Title, string Artist, int Year,
    int TrackCount, int DurationMinutes, int GenreId);