using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TokoBuku.Application.Common.Interfaces;
using TokoBuku.Application.Features.Purchases;

namespace TokoBuku.Web.Controllers;

[Authorize(Roles = "Admin")]
public class PurchaseController : Controller
{
    private readonly ISender _sender;
    private readonly IApplicationDbContext _context;

    public PurchaseController(ISender sender, IApplicationDbContext context)
    {
        _sender = sender;
        _context = context;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? supplierId, int page = 1)
    {
        var result = await _sender.Send(new GetPurchasesQuery(startDate, endDate, supplierId, page, 10));
        var suppliers = await _context.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync();

        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
        ViewBag.SupplierId = supplierId;
        ViewBag.Suppliers = suppliers;

        return View(result);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var suppliers = await _context.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToListAsync();
        var books = await _context.Books.AsNoTracking().OrderBy(b => b.Title).ToListAsync();

        ViewBag.Suppliers = suppliers;
        ViewBag.Books = books;

        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int supplierId, string? notes, List<int> bookId, List<int> quantity, List<decimal> unitCost)
    {
        var items = new List<PurchaseItemInput>();
        for (int i = 0; i < bookId.Count; i++)
        {
            if (bookId[i] > 0 && quantity[i] > 0)
            {
                items.Add(new PurchaseItemInput(bookId[i], quantity[i], unitCost[i]));
            }
        }

        if (items.Count == 0)
        {
            TempData["Error"] = "Pilih minimal 1 buku untuk pengadaan.";
            return RedirectToAction(nameof(Create));
        }

        try
        {
            var command = new CreatePurchaseCommand(supplierId, notes, items);
            var purchase = await _sender.Send(command);

            TempData["Success"] = $"Pengadaan #{purchase.Id} berhasil dicatat dan stok buku telah diperbarui!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToAction(nameof(Create));
        }
    }
}
