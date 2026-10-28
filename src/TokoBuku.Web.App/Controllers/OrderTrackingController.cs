using MediatR;
using Microsoft.AspNetCore.Mvc;
using TokoBuku.Application.Features.Sales.Queries;
using TokoBuku.Web.App.Models;
using TokoBuku.Web.App.ViewModels;

namespace TokoBuku.Web.App.Controllers;

public class OrderTrackingController : Controller
{
    private readonly ISender _sender;

    public OrderTrackingController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? invoiceNumber)
    {
        var model = new OrderTrackingViewModel
        {
            SearchInvoice = invoiceNumber
        };

        if (string.IsNullOrWhiteSpace(invoiceNumber))
        {
            return View(model);
        }

        model.HasSearched = true;
        var cleanInvoice = invoiceNumber.Trim();
        var sale = await _sender.Send(new GetSaleByIdQuery(cleanInvoice));

        if (sale == null)
        {
            model.IsFound = false;
            model.ErrorMessage = $"Faktur pesanan dengan nomor \"{cleanInvoice}\" tidak ditemukan dalam database.";
            return View(model);
        }

        model.IsFound = true;
        model.InvoiceNumber = sale.Id;
        model.TransactionDate = sale.TransactionDate;
        model.CustomerName = sale.CustomerName;
        model.NetAmount = sale.GrandTotal;
        model.Status = "Lunas & Terkonfirmasi";
        model.Items = sale.Details.Select(i => new CartItem
        {
            BookId = i.BookId,
            Title = i.BookTitle,
            UnitPrice = i.UnitPrice,
            DiscountPercent = i.DiscountPercentage,
            Quantity = i.Quantity
        }).ToList();

        return View(model);
    }
}
