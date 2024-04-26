namespace MangaDexWebApi.Models;
public class Manga
{
    public string Id { get; set; } = String.Empty;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public bool IsLocked { get; set; }
    public string[]? AvailableLanguages { get; set; }
    public string? Links { get; set; }
    public string? OriginalLanguage { get; set; }
    public string? LastVolume { get; set; }
    public string? LastChapter { get; set; }
    public int? Year { get; set; }
    public IEnumerable<string>? Tags { get; set; }
    public string? State { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? LatestUploadedChapter { get; set; }
    public string? Cover { get; set; }
}