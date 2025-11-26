using System.Diagnostics;
using GasStationSystem.Web.Data;
using Microsoft.AspNetCore.Mvc;
using GasStationSystem.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace GasStationSystem.Web.Controllers;

/// <summary>
/// Serves as the entry point for the application's public-facing pages.
/// Handles the display of the product catalog (Home), privacy policy, and error pages.
/// </summary>
public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    /// <summary>
    /// Displays the main landing page (Catalog).
    /// Retrieves a list of all available products (Fuel and Food) from the database
    /// to be displayed to the customer.
    /// </summary>
    /// <returns>The Index view populated with a list of products.</returns>
    public async Task<IActionResult> Index()
    {
        var products = await _context.Products.ToListAsync();
        return View(products);
    }

    /// <summary>
    /// Displays the Privacy Policy page containing legal information about data handling.
    /// </summary>
    /// <returns>The Privacy view.</returns>
    public IActionResult Privacy()
    {
        return View();
    }

    /// <summary>
    /// Displays the global Error page with diagnostic information.
    /// Response caching is disabled to ensure real-time error details are shown to the user.
    /// </summary>
    /// <returns>The Error view containing the Request ID for debugging.</returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}