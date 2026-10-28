using MediatR;
using Microsoft.AspNetCore.Mvc;
using TokoBuku.Application.Features.Books.Queries;
using TokoBuku.Application.Features.Categories;
using TokoBuku.Web.App.ViewModels;

namespace TokoBuku.Web.App.Controllers;

public class HomeController : Controller
{
    private readonly ISender _sender;

    public HomeController(ISender sender)
    {
        _sender = sender;
    }

    public async Task<IActionResult> Index()
    {
        var categoriesResult = await _sender.Send(new GetCategoriesQuery());
        var booksResult = await _sender.Send(new GetBooksQuery(PageSize: 20));

        var books = booksResult.Items;

        var viewModel = new HomeViewModel
        {
            Categories = categoriesResult.Select(c => c.Name).ToList(),
            LatestBooks = books.OrderByDescending(b => b.Id).Take(8).ToList(),
            DiscountedBooks = books.Where(b => b.DiscountPercentage > 0).OrderByDescending(b => b.DiscountPercentage).Take(8).ToList(),
            BestSellers = books.OrderByDescending(b => b.Price).Take(8).ToList()
        };

        return View(viewModel);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
