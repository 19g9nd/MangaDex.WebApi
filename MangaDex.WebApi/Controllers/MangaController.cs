using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MangaDexWebApi.ViewModels;
using MangaFlex.Api.ViewModels;
using MangaDexWebApi.Services.Base;
using MangaDexWebApi.Models;

namespace MangaDexWebApi.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]/[action]")]
public class MangaController : Controller
{
    private readonly IMangaService mangaService;

    public MangaController(IMangaService mangaService)
    {
        this.mangaService = mangaService;
    }
    
    [HttpGet]
    [AllowAnonymous]
    [Route("/api/Mangas")]
    public async Task<IActionResult> Mangas(int page = 1, string? search = null)
    {
        MangasViewModel mangasViewModel;
        try
        {
            mangasViewModel = await this.mangaService.FindMangasAsync(search, page);
        }
        catch (AggregateException ex)
        {
            foreach (ArgumentException error in ex.Flatten().InnerExceptions)
            {
                System.Console.WriteLine(error.Message);
            }
            return BadRequest("While searching for this request an error happened");
        }
        return Ok(mangasViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> AvailableLanguages(string id)
    {
        Console.WriteLine(id);
        var availableLanguages = await mangaService.GetAvailableLanguages(id);
        var AvailableLanguagesVm = new AvailableLanguagesVM()
        {
            Id = id,
            Languages = availableLanguages
        };
        return Ok(AvailableLanguagesVm);
    }

    [HttpGet]
    public async Task<IActionResult> About(string id)
    {
        Manga manga;
        
        try
        {
            manga = await mangaService.GetByIdAsync(id);
        }
        catch
        {
            return BadRequest("An unexpected error occurred.");
        }

        return Ok(manga);
    }

    [HttpGet]
    public async Task<IActionResult> Read(string id, int chapter = 1, string language = "en")
    {
        var mangaChapterViewModel = await mangaService.ReadAsync(id, chapter, language);
        return Ok(mangaChapterViewModel);
    }
}  