using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TokoBuku.Application.Common.Interfaces;
using TokoBuku.Application.Features.Books.Commands;
using TokoBuku.Application.Features.Books.Queries;
using TokoBuku.Application.Features.Categories;

namespace TokoBuku.Web.Controllers;

[Authorize]
public class BookController : Controller
{
    private readonly ISender _sender;
    private readonly IFileStorageService _fileStorage;
    private readonly IExportService _exportService;
    private readonly IApplicationDbContext _context;

    public BookController(
        ISender sender,
        IFileStorageService fileStorage,
        IExportService exportService,
        IApplicationDbContext context)
    {
        _sender = sender;
        _fileStorage = fileStorage;
        _exportService = exportService;
        _context = context;
    }

    public async Task<IActionResult> Index(string? search, int? categoryId, int page = 1)
    {
        var result = await _sender.Send(new GetBooksQuery(search, categoryId, page, 10));
        var categories = await _sender.Send(new GetCategoriesQuery());

        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.Categories = new SelectList(categories, "Id", "Name", categoryId);

        return View(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create()
    {
        var categories = await _sender.Send(new GetCategoriesQuery());
        ViewBag.Categories = new SelectList(categories, "Id", "Name");
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateBookCommand command, IFormFile? coverFile)
    {
        var categories = await _sender.Send(new GetCategoriesQuery());
        ViewBag.Categories = new SelectList(categories, "Id", "Name", command.CategoryId);

        if (!ModelState.IsValid)
            return View(command);

        try
        {
            string? coverImageName = "default.jpg";
            if (coverFile != null && coverFile.Length > 0)
            {
                using var stream = coverFile.OpenReadStream();
                coverImageName = await _fileStorage.SaveFileAsync(stream, coverFile.FileName, "covers");
            }

            var finalCommand = command with { CoverImage = coverImageName };
            await _sender.Send(finalCommand);

            TempData["Success"] = "Data buku berhasil ditambahkan!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(command);
        }
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Edit(int id)
    {
        var book = await _sender.Send(new GetBookByIdQuery(id));
        if (book == null) return NotFound();

        var categories = await _sender.Send(new GetCategoriesQuery());
        ViewBag.Categories = new SelectList(categories, "Id", "Name", book.CategoryId);

        var command = new UpdateBookCommand(
            book.Id,
            book.Title,
            book.Author,
            book.ReleaseYear,
            book.Price,
            book.DiscountPercentage,
            book.Stock,
            book.CategoryId,
            book.CoverImage
        );

        return View(command);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateBookCommand command, IFormFile? coverFile)
    {
        var categories = await _sender.Send(new GetCategoriesQuery());
        ViewBag.Categories = new SelectList(categories, "Id", "Name", command.CategoryId);

        if (!ModelState.IsValid)
            return View(command);

        try
        {
            string? coverImageName = command.CoverImage;
            if (coverFile != null && coverFile.Length > 0)
            {
                using var stream = coverFile.OpenReadStream();
                coverImageName = await _fileStorage.SaveFileAsync(stream, coverFile.FileName, "covers");
            }

            var finalCommand = command with { CoverImage = coverImageName };
            await _sender.Send(finalCommand);

            TempData["Success"] = "Data buku berhasil diperbarui!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(command);
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _sender.Send(new DeleteBookCommand(id));
            TempData["Success"] = "Buku berhasil dihapus.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel()
    {
        var books = await _context.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .OrderBy(b => b.Title)
            .ToListAsync();

        var bytes = _exportService.ExportBooksToExcel(books);
        return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Data-Buku-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
