using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TokoBuku.Application.Common.Interfaces;
using TokoBuku.Application.Features.Sales.Queries;

namespace TokoBuku.Web.Controllers;

[Authorize]
public class SalesController : Controller
{
    private readonly ISender _sender;
    private readonly IApplicationDbContext _context;
    private readonly IExportService _exportService;

    public SalesController(ISender sender, IApplicationDbContext context, IExportService exportService)
    {
        _sender = sender;
        _context = context;
        _exportService = exportService;
    }

    public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate, int? customerId, int page = 1)
    {
        var result = await _sender.Send(new GetSalesQuery(startDate, endDate, customerId, page, 10));

        ViewBag.StartDate = startDate?.ToString("yyyy-MM-dd");
        ViewBag.EndDate = endDate?.ToString("yyyy-MM-dd");
        ViewBag.CustomerId = customerId;

        return View(result);
    }

    public async Task<IActionResult> Detail(string id)
    {
        var sale = await _sender.Send(new GetSaleByIdQuery(id));
        if (sale == null) return NotFound();

        return View(sale);
    }

    [HttpGet]
    public async Task<IActionResult> ReceiptPdf(string id)
    {
        var sale = await _context.Sales
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
                .ThenInclude(d => d.Book)
            .FirstOrDefaultAsync(s => s.Id == id);

        if (sale == null) return NotFound();

        var pdfBytes = _exportService.GenerateReceiptPdf(sale);
        return File(pdfBytes, "application/pdf", $"Struk-{sale.Id}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportPdf(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
                .ThenInclude(d => d.Book)
            .AsQueryable();

        if (startDate.HasValue) query = query.Where(s => s.TransactionDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(s => s.TransactionDate <= endDate.Value);

        var sales = await query.OrderByDescending(s => s.TransactionDate).ToListAsync();
        var pdfBytes = _exportService.GenerateSalesReportPdf(sales, startDate, endDate);

        return File(pdfBytes, "application/pdf", $"Laporan-Penjualan-{DateTime.UtcNow:yyyyMMdd}.pdf");
    }

    [HttpGet]
    public async Task<IActionResult> ExportExcel(DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Sales
            .AsNoTracking()
            .Include(s => s.Customer)
            .Include(s => s.SaleDetails)
                .ThenInclude(d => d.Book)
            .AsQueryable();

        if (startDate.HasValue) query = query.Where(s => s.TransactionDate >= startDate.Value);
        if (endDate.HasValue) query = query.Where(s => s.TransactionDate <= endDate.Value);

        var sales = await query.OrderByDescending(s => s.TransactionDate).ToListAsync();
        var excelBytes = _exportService.ExportSalesToExcel(sales);

        return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Laporan-Penjualan-{DateTime.UtcNow:yyyyMMdd}.xlsx");
    }
}
