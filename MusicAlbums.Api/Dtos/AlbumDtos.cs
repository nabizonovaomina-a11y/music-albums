 namespace MusicAlbums.Api.Dtos;

public record GenreDto(int Id, string Name);

public record AlbumDto(
    int Id, string Title, string Artist, int Year,
    int TrackCount, int DurationMinutes,
    int GenreId, string GenreName, string? CoverUrl, int OwnerId);

// POST va PUT uchun
public record AlbumRequest(
    string Title, string Artist, int Year,
    int TrackCount, int DurationMinutes, int GenreId);

// GET list uchun: filtr, saralash, sahifalash
public record AlbumQuery(
    int? GenreId, string? Artist, int? Year,
    string? Sort, string? Dir, int? Page, int? PageSize);

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int Total);