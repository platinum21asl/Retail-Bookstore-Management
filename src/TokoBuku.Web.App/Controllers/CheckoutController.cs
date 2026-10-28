using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TokoBuku.Application.Common.Interfaces;
using TokoBuku.Application.Features.Sales.Commands;
using TokoBuku.Application.Features.Sales.DTOs;
using TokoBuku.Application.Features.Sales.Queries;
using TokoBuku.Domain.Entities;
using TokoBuku.Domain.Enums;
using TokoBuku.Domain.Exceptions;
using TokoBuku.Web.App.Models;
using TokoBuku.Web.App.Services;
using TokoBuku.Web.App.ViewModels;

namespace TokoBuku.Web.App.Controllers;

public class CheckoutController : Controller
{
    private readonly ICartService _cartService;
    private readonly ISender _sender;
    private readonly IApplicationDbContext _context;

    public CheckoutController(ICartService cartService, ISender sender, IApplicationDbContext context)
    {
        _cartService = cartService;
        _sender = sender;
        _context = context;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var cart = _cartService.GetCart();
        if (cart.Items.Count == 0)
        {
            TempData["Error"] = "Keranjang belanja Anda masih kosong. Silakan pilih buku terlebih dahulu.";
            return RedirectToAction("Index", "Cart");
        }

        var viewModel = new CheckoutViewModel
        {
            Cart = cart,
            FulfillmentMethod = "StorePickUp",
            PaymentMethod = "QRIS"
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Index(CheckoutViewModel model)
    {
        var cart = _cartService.GetCart();
        if (cart.Items.Count == 0)
        {
            TempData["Error"] = "Keranjang belanja Anda kosong.";
            return RedirectToAction("Index", "Catalog");
        }

        model.Cart = cart;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // 1. Retrieve or register Customer profile
            var normalizedEmail = model.CustomerEmail.Trim().ToLower();
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Email.ToLower() == normalizedEmail);

            if (customer == null)
            {
                customer = new Customer
                {
                    CustomerNumber = $"CUST-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
                    Name = model.CustomerName.Trim(),
                    Email = model.CustomerEmail.Trim(),
                    Phone = model.CustomerPhone?.Trim() ?? string.Empty,
                    Address = model.ShippingAddress?.Trim() ?? (model.FulfillmentMethod == "StorePickUp" ? "Ambil Langsung di Toko" : string.Empty),
                    Gender = Gender.Male
                };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }
            else
            {
                // Update contact phone/address if newly provided
                if (!string.IsNullOrWhiteSpace(model.CustomerPhone))
                    customer.Phone = model.CustomerPhone.Trim();

                if (!string.IsNullOrWhiteSpace(model.ShippingAddress))
                    customer.Address = model.ShippingAddress.Trim();

                await _context.SaveChangesAsync();
            }

            // 2. Create Sales Transaction via CQRS Command
            var itemsInput = cart.Items
                .Select(i => new CartItemInput(i.BookId, i.Quantity))
                .ToList();

            var command = new CreateSaleCommand(
                CustomerId: customer.Id,
                CashPaid: cart.TotalNetAmount,
                Items: itemsInput
            );

            var saleResult = await _sender.Send(command);

            // Store order metadata in session for confirmation receipt
            HttpContext.Session.SetString($"LAST_ORDER_METHOD_{saleResult.Id}", model.FulfillmentMethod);
            HttpContext.Session.SetString($"LAST_ORDER_PAYMENT_{saleResult.Id}", model.PaymentMethod);
            HttpContext.Session.SetString($"LAST_ORDER_SHIPPING_{saleResult.Id}", model.ShippingAddress ?? "Ambil di Toko");

            // 3. Clear Shopping Cart
            _cartService.ClearCart();

            return RedirectToAction(nameof(Success), new { id = saleResult.Id });
        }
        catch (DomainException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Terjadi kesalahan saat memproses pesanan: {ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Success(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return RedirectToAction("Index", "Home");
        }

        var sale = await _sender.Send(new GetSaleByIdQuery(id));
        if (sale == null)
        {
            return NotFound();
        }

        var fulfillment = HttpContext.Session.GetString($"LAST_ORDER_METHOD_{id}") ?? "StorePickUp";
        var payment = HttpContext.Session.GetString($"LAST_ORDER_PAYMENT_{id}") ?? "QRIS";
        var shipping = HttpContext.Session.GetString($"LAST_ORDER_SHIPPING_{id}") ?? "-";

        var viewModel = new OrderSuccessViewModel
        {
            InvoiceNumber = sale.Id,
            TransactionDate = sale.TransactionDate,
            CustomerName = sale.CustomerName ?? "Pelanggan Umum",
            CustomerEmail = "-",
            FulfillmentMethod = fulfillment,
            PaymentMethod = payment,
            ShippingAddress = shipping,
            TotalAmount = sale.Subtotal,
            DiscountAmount = sale.TotalDiscount,
            NetAmount = sale.GrandTotal,
            Items = sale.Details.Select(i => new CartItem
            {
                BookId = i.BookId,
                Title = i.BookTitle,
                UnitPrice = i.UnitPrice,
                DiscountPercent = i.DiscountPercentage,
                Quantity = i.Quantity
            }).ToList()
        };

        return View(viewModel);
    }
}
