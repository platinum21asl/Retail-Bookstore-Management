using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TokoBuku.Application.Features.Customers;

namespace TokoBuku.Web.Controllers;

[Authorize]
public class CustomerController : Controller
{
    private readonly ISender _sender;

    public CustomerController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IActionResult> Index(string? search, int page = 1)
    {
        var result = await _sender.Send(new GetCustomersQuery(search, page, 10));
        ViewBag.Search = search;
        return View(result);
    }

    [HttpGet]
    [Authorize(Roles = "Admin")]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateCustomerCommand command)
    {
        if (!ModelState.IsValid) return View(command);

        try
        {
            await _sender.Send(command);
            TempData["Success"] = "Pelanggan berhasil ditambahkan.";
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
        var result = await _sender.Send(new GetCustomersQuery(PageSize: 100));
        var cust = result.Items.FirstOrDefault(c => c.Id == id);
        if (cust == null) return NotFound();

        var command = new UpdateCustomerCommand(cust.Id, cust.Name, cust.Gender, cust.Address, cust.Email, cust.Phone);
        return View(command);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateCustomerCommand command)
    {
        if (!ModelState.IsValid) return View(command);

        try
        {
            await _sender.Send(command);
            TempData["Success"] = "Data pelanggan berhasil diperbarui.";
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
            await _sender.Send(new DeleteCustomerCommand(id));
            TempData["Success"] = "Pelanggan berhasil dihapus.";
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
