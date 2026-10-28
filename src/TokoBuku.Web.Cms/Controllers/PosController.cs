using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TokoBuku.Application.Common.Interfaces;
using TokoBuku.Application.Features.Categories;
using TokoBuku.Application.Features.Sales.Commands;
using TokoBuku.Domain.Exceptions;

namespace TokoBuku.Web.Controllers;

[Authorize]
public class PosController : Controller
{
    private readonly ISender _sender;
    private readonly IApplicationDbContext _context;

    public PosController(ISender sender, IApplicationDbContext context)
    {
        _sender = sender;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var books = await _context.Books
            .AsNoTracking()
            .Include(b => b.Category)
            .OrderByDescending(b => b.Stock > 0)
            .ThenBy(b => b.Title)
            .ToListAsync();

        var categories = await _sender.Send(new GetCategoriesQuery());
        var customers = await _context.Customers
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.Categories = categories;
        ViewBag.Customers = customers;

        return View(books);
    }

    [HttpPost]
    [IgnoreAntiforgeryToken] // JSON API for modern reactive POS UI
    public async Task<IActionResult> Checkout([FromBody] CreateSaleCommand command)
    {
        try
        {
            var sale = await _sender.Send(command);
            return Json(new
            {
                success = true,
                message = "Transaksi pembayaran berhasil!",
                sale
            });
        }
        catch (DomainException ex)
        {
            return BadRequest(new { success = false, message = ex.Message });
        }
        catch (FluentValidation.ValidationException ex)
        {
            var errors = string.Join("; ", ex.Errors.Select(e => e.ErrorMessage));
            return BadRequest(new { success = false, message = errors });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { success = false, message = "Terjadi kesalahan internal: " + ex.Message });
        }
    }
}
