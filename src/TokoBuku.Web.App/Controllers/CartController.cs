using MediatR;
using Microsoft.AspNetCore.Mvc;
using TokoBuku.Application.Features.Books.Queries;
using TokoBuku.Web.App.Services;

namespace TokoBuku.Web.App.Controllers;

public class CartController : Controller
{
    private readonly ICartService _cartService;
    private readonly ISender _sender;

    public CartController(ICartService cartService, ISender sender)
    {
        _cartService = cartService;
        _sender = sender;
    }

    public IActionResult Index()
    {
        var cart = _cartService.GetCart();
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int bookId, int quantity = 1)
    {
        var book = await _sender.Send(new GetBookByIdQuery(bookId));
        if (book != null && book.Stock > 0)
        {
            _cartService.AddItem(
                book.Id,
                book.Title,
                book.Author,
                book.CoverImage,
                book.Price,
                book.DiscountPercentage,
                quantity
            );
            TempData["Success"] = $"Buku \"{book.Title}\" berhasil ditambahkan ke keranjang.";
        }
        else
        {
            TempData["Error"] = "Stok buku tidak mencukupi atau buku tidak ditemukan.";
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddToCartAjax([FromBody] AddToCartRequest request)
    {
        var book = await _sender.Send(new GetBookByIdQuery(request.BookId));
        if (book == null)
        {
            return Json(new { success = false, message = "Buku tidak ditemukan." });
        }

        if (book.Stock < request.Quantity)
        {
            return Json(new { success = false, message = $"Stok buku tidak mencukupi (Tersisa {book.Stock})." });
        }

        _cartService.AddItem(
            book.Id,
            book.Title,
            book.Author,
            book.CoverImage,
            book.Price,
            book.DiscountPercentage,
            request.Quantity
        );

        var cart = _cartService.GetCart();

        return Json(new
        {
            success = true,
            message = $"\"{book.Title}\" berhasil ditambahkan ke keranjang.",
            totalItems = cart.TotalItems,
            totalNetAmount = cart.TotalNetAmount,
            cart
        });
    }

    [HttpPost]
    public IActionResult UpdateQty(int bookId, int quantity)
    {
        _cartService.UpdateQuantity(bookId, quantity);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult UpdateQtyAjax([FromBody] UpdateQtyRequest request)
    {
        _cartService.UpdateQuantity(request.BookId, request.Quantity);
        var cart = _cartService.GetCart();
        return Json(new
        {
            success = true,
            totalItems = cart.TotalItems,
            totalOriginalAmount = cart.TotalOriginalAmount,
            totalDiscountAmount = cart.TotalDiscountAmount,
            totalNetAmount = cart.TotalNetAmount,
            cart
        });
    }

    [HttpPost]
    public IActionResult RemoveItem(int bookId)
    {
        _cartService.RemoveItem(bookId);
        TempData["Success"] = "Item berhasil dihapus dari keranjang.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public IActionResult RemoveItemAjax([FromBody] RemoveItemRequest request)
    {
        _cartService.RemoveItem(request.BookId);
        var cart = _cartService.GetCart();
        return Json(new
        {
            success = true,
            totalItems = cart.TotalItems,
            totalNetAmount = cart.TotalNetAmount,
            cart
        });
    }

    [HttpGet]
    public IActionResult GetSummaryAjax()
    {
        var cart = _cartService.GetCart();
        return Json(new
        {
            totalItems = cart.TotalItems,
            totalOriginalAmount = cart.TotalOriginalAmount,
            totalDiscountAmount = cart.TotalDiscountAmount,
            totalNetAmount = cart.TotalNetAmount,
            items = cart.Items
        });
    }
}

public record AddToCartRequest(int BookId, int Quantity);
public record UpdateQtyRequest(int BookId, int Quantity);
public record RemoveItemRequest(int BookId);
