using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TokoBuku.Application.Features.Categories;

namespace TokoBuku.Web.Controllers;

[Authorize(Roles = "Admin")]
public class CategoryController : Controller
{
    private readonly ISender _sender;

    public CategoryController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IActionResult> Index()
    {
        var categories = await _sender.Send(new GetCategoriesQuery());
        return View(categories);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCategoryCommand command)
    {
        if (!ModelState.IsValid)
        {
            TempData["Error"] = "Nama kategori tidak boleh kosong.";
            return RedirectToAction(nameof(Index));
        }

        try
        {
            await _sender.Send(command);
            TempData["Success"] = "Kategori berhasil ditambahkan.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _sender.Send(new DeleteCategoryCommand(id));
            TempData["Success"] = "Kategori berhasil dihapus.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
