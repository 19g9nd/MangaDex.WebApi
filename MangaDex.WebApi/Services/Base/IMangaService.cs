using MangaDexWebApi.Models;
using MangaDexWebApi.ViewModels;

namespace MangaDexWebApi.Services.Base;
public interface IMangaService
{
    public Task<MangasViewModel> FindMangasAsync(string? query = null, int page = 1);
    public Task<Manga> GetByIdAsync(string id);
    public Task<MangaChapterViewModel> ReadAsync(string mangaId, int chapter = 1, string language = "en");
    public Task<string[]> GetAvailableLanguages(string id);
}