using MangaDexSharp;
using MangaDexWebApi.ViewModels;

namespace MangaDexWebApi.Services;

using MangaDexWebApi.Models;
using MangaDexWebApi.Services.Base;

public class MangaService : IMangaService
{
    private IMangaDex apiClient;

    public MangaService()
    {
        apiClient = MangaDex.Create();
    }

    public async Task<MangasViewModel> FindMangasAsync(string? query = null, int page = 1)
    {
        var mangaFilter = new MangaFilter
        {
            Title = query ?? string.Empty,
            Limit = 20,
            Offset = 20 * (page - 1),
            ContentRating = new[] { ContentRating.safe },
            AvailableTranslatedLanguage = new[] { "en" },
            HasAvailableChapters = true,
        };

        mangaFilter.Order[MangaFilter.OrderKey.rating] = OrderValue.desc;

        MangaList mangaList = await this.apiClient.Manga.List(mangaFilter);

        if (mangaList.ErrorOccurred)
        {
            var exceptions = new List<Exception>();

            foreach (var error in mangaList.Errors)
            {
                exceptions.Add(new ArgumentException(error.Detail, error.Title));
            }

            throw new AggregateException(exceptions);
        }

        var allMangas = new List<Manga>();
        foreach (var manga in mangaList.Data)
        {
            allMangas.Add(this.Convert(manga, manga.CoverArt().FirstOrDefault()?.Attributes?.FileName));
        }

        var mangasViewModel = new MangasViewModel
        {
            Mangas = allMangas,
            Page = page,
            TotalPages = mangaList.Total / 20,
            Search = query,
        };

        return mangasViewModel;
    }

    public async Task<string[]> GetAvailableLanguages(string id)
    {
        var manga = await this.apiClient.Manga.Get(id);
        var result = manga?.Data?.Attributes?.AvailableTranslatedLanguages;
        return result!;
    }

    public async Task<Manga> GetByIdAsync(string id)
    {
        var result = await this.apiClient.Manga.Get(id);
        if (result.ErrorOccurred)
        {
            var exceptions = new List<Exception>();

            foreach (var error in result.Errors)
            {
                exceptions.Add(new ArgumentException(error.Detail, error.Title));
            }

            throw new AggregateException(exceptions);
        }
        var coverFileName = result.Data.CoverArt().FirstOrDefault()?.Attributes?.FileName;
        return this.Convert(result.Data, coverFileName);
    }
    public async Task<MangaChapterViewModel> ReadAsync(string mangaId, int chapter = 1, string language = "en")
    {
        var mangaPages = new List<string>();

        var mangaFeedFilter = new MangaFeedFilter
        {
            Order = new()
            {
                [MangaFeedFilter.OrderKey.volume] = OrderValue.asc,
                [MangaFeedFilter.OrderKey.chapter] = OrderValue.asc,
            },
        };
        mangaFeedFilter.TranslatedLanguage = new[] { language };
        // Fetch manga chapters
        var chapters = await apiClient.Manga.Feed(mangaId, mangaFeedFilter);
        // Filter chapters by language
        var chaptersInLanguage = chapters.Data?.Where(chapter => chapter.Attributes?.TranslatedLanguage == language);
        // Fetch pages for the specified chapter
        var pages = await apiClient.Pages.Pages(chapterId: chapters.Data?[chapter - 1]?.Id!);
        var currentChapter = await apiClient.Chapter.Get(chapters.Data?[chapter - 1]?.Id!);
        var volume = currentChapter.Data.Attributes?.Volume;
        var chapterTitle = currentChapter.Data.Attributes?.Title;
        var mangaChapterViewModel = new MangaChapterViewModel
        {
            MangaId = mangaId,
            Pages = pages?.GenerateImageLinks() ?? Enumerable.Empty<string>(),
            Chapter = chapter,
            TotalChapters = chaptersInLanguage?.Count() ?? 0,
            ChapterTitle = chapterTitle,
            Volume = volume,
        };

        return mangaChapterViewModel;
    }

    private Manga Convert(MangaDexSharp.Manga mangaToConvert, string? coverFileName) => new Manga
    {
        Id = mangaToConvert.Id,
        Title = mangaToConvert.Attributes?.Title.FirstOrDefault().Value,
        Description = mangaToConvert.Attributes?.Description.FirstOrDefault().Value,
        AvailableLanguages = mangaToConvert.Attributes?.AvailableTranslatedLanguages,
        IsLocked = mangaToConvert.Attributes?.IsLocked ?? false,
        OriginalLanguage = mangaToConvert.Attributes?.OriginalLanguage,
        LastVolume = mangaToConvert.Attributes?.LastVolume,
        LastChapter = mangaToConvert.Attributes?.LastChapter,
        Year = mangaToConvert.Attributes?.Year,
        Tags = mangaToConvert.Attributes?.Tags.Select(mg => mg.Attributes!.Name.FirstOrDefault().Value),
        State = mangaToConvert.Attributes?.State,
        CreatedAt = mangaToConvert.Attributes?.CreatedAt,
        UpdatedAt = mangaToConvert.Attributes?.UpdatedAt,
        LatestUploadedChapter = mangaToConvert.Attributes?.LatestUploadedChapter,
        Cover = $"https://uploads.mangadex.org/covers/{mangaToConvert.Id}/{coverFileName}",
    };
}
