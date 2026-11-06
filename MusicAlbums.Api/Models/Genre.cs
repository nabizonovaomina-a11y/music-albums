namespace MusicAlbums.Api.Models;

public class Genre
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Album> Albums { get; set; } = [];
}
