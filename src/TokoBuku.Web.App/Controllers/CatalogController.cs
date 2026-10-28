using MediatR;
using Microsoft.AspNetCore.Mvc;
using TokoBuku.Application.Common.Models;
using TokoBuku.Application.Features.Books.DTOs;
using TokoBuku.Application.Features.Books.Queries;
using TokoBuku.Application.Features.Categories;
using TokoBuku.Web.App.ViewModels;

namespace TokoBuku.Web.App.Controllers;

public class CatalogController : Controller
{
    private readonly ISender _sender;

    public CatalogController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IActionResult> Index(string? category, string? search, string? sort, int page = 1)
    {
        var categories = await _sender.Send(new GetCategoriesQuery());
        
        int? categoryId = null;
        if (!string.IsNullOrWhiteSpace(category))
        {
            var match = categories.FirstOrDefault(c => c.Name.Equals(category, StringComparison.OrdinalIgnoreCase));
            if (match != null)
            {
                categoryId = match.Id;
            }
        }

        var booksResult = await _sender.Send(new GetBooksQuery(
            SearchTerm: search,
            CategoryId: categoryId,
            PageNumber: page,
            PageSize: 12
        ));

        var items = booksResult.Items.ToList();

        // In-memory sorting for display
        items = sort switch
        {
            "price_asc" => items.OrderBy(b => b.DiscountedPrice).ToList(),
            "price_desc" => items.OrderByDescending(b => b.DiscountedPrice).ToList(),
            "title_asc" => items.OrderBy(b => b.Title).ToList(),
            "discount_desc" => items.OrderByDescending(b => b.DiscountPercentage).ToList(),
            _ => items.OrderByDescending(b => b.Id).ToList()
        };

        var paginatedBooks = new PaginatedList<BookDto>(items, booksResult.TotalCount, page, 12);

        var viewModel = new CatalogViewModel
        {
            Books = paginatedBooks,
            Categories = categories.Select(c => c.Name).ToList(),
            SelectedCategory = category,
            Search = search,
            Sort = sort
        };

        return View(viewModel);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var book = await _sender.Send(new GetBookByIdQuery(id));
        if (book == null)
        {
            return NotFound();
        }

        // Retrieve recommended / related books in the same category
        var relatedResult = await _sender.Send(new GetBooksQuery(
            CategoryId: book.CategoryId,
            PageSize: 5
        ));

        var relatedBooks = relatedResult.Items
            .Where(b => b.Id != id)
            .Take(4)
            .ToList();

        var viewModel = new BookDetailViewModel
        {
            Book = book,
            RelatedBooks = relatedBooks
        };

        return View(viewModel);
    }
}
