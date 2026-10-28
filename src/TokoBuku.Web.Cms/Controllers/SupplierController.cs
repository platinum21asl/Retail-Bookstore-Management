using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TokoBuku.Application.Features.Suppliers;

namespace TokoBuku.Web.Controllers;

[Authorize(Roles = "Admin")]
public class SupplierController : Controller
{
    private readonly ISender _sender;

    public SupplierController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _sender.Send(new GetSuppliersQuery(search, page, 10));
        ViewBag.Search = search;
        return View(result);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSupplierCommand command)
    {
        if (!ModelState.IsValid) return View(command);

        try
        {
            await _sender.Send(command);
            TempData["Success"] = "Supplier berhasil ditambahkan.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(command);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var result = await _sender.Send(new GetSuppliersQuery(PageSize: 100));
        var item = result.Items.FirstOrDefault(s => s.Id == id);
        if (item == null) return NotFound();

        var command = new UpdateSupplierCommand(item.Id, item.Name, item.Address, item.Email, item.Phone);
        return View(command);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateSupplierCommand command)
    {
        if (!ModelState.IsValid) return View(command);

        try
        {
            await _sender.Send(command);
            TempData["Success"] = "Data supplier berhasil diperbarui.";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(command);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _sender.Send(new DeleteSupplierCommand(id));
            TempData["Success"] = "Supplier berhasil dihapus.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
