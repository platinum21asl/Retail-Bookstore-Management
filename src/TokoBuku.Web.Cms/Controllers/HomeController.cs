using System.Diagnostics;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TokoBuku.Application.Features.Sales.Queries;
using TokoBuku.Web.Models;

namespace TokoBuku.Web.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly ISender _sender;
    private readonly IWebHostEnvironment _env;

    public HomeController(ISender sender, IWebHostEnvironment env)
    {
        _sender = sender;
        _env = env;
    }

    public async Task<IActionResult> Index(int? year)
    {
        var targetYear = year ?? DateTime.UtcNow.Year;
        var stats = await _sender.Send(new GetDashboardStatsQuery(targetYear));
        ViewBag.SelectedYear = targetYear;
        return View(stats);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error(string? traceId)
    {
        var requestId = traceId ?? Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var ex = exceptionFeature?.Error ?? HttpContext.Items["LastError"] as Exception;

        var isDevOrAdmin = _env.IsDevelopment() || (User.Identity?.IsAuthenticated == true && User.IsInRole("Admin"));

        var model = new ErrorViewModel
        {
            RequestId = requestId,
            Timestamp = DateTime.UtcNow.ToString("dd MMMM yyyy, HH:mm:ss 'UTC'"),
            Path = exceptionFeature?.Path ?? HttpContext.Request.Path,
            HttpMethod = HttpContext.Request.Method,
            IsDeveloperOrAdmin = isDevOrAdmin,
            ExceptionType = ex?.GetType().FullName,
            ExceptionMessage = ex?.Message,
            StackTrace = ex?.StackTrace,
            InnerExceptionMessage = ex?.InnerException?.Message
        };

        return View(model);
    }
}
