 namespace MusicAlbums.Api.Models;

public class Album
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Artist { get; set; } = "";
    public int Year { get; set; }
    public int TrackCount { get; set; }
    public int DurationMinutes { get; set; }
    public string? CoverFileName { get; set; }

    public int GenreId { get; set; }
    public Genre? Genre { get; set; }

    public int OwnerId { get; set; }
    public User? Owner { get; set; }
}