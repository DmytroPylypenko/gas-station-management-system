using GasStationSystem.Web.Data;
using GasStationSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GasStationSystem.Web.Controllers;

/// <summary>
/// Provides cashier-facing functionalities such as viewing orders,
/// checking inventory levels, and generating printable receipts.
/// </summary>
[Authorize(Roles = "Cashier,Admin")]
public class CashierController : Controller
{
    private readonly ApplicationDbContext _context;

    public CashierController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Displays a list of all orders made in the system.
    /// Includes related user information and order item details.
    /// Orders are sorted by creation date in descending order.
    /// </summary>
    /// <returns>A view containing the list of orders.</returns>
    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
            .Include(o => o.User) 
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .OrderByDescending(o => o.CreatedAt) 
            .ToListAsync();

        return View(orders);
    }

    /// <summary>
    /// Displays the available products along with their stock quantities.
    /// Sorted from lowest to highest quantity to highlight low-stock items.
    /// </summary>
    /// <returns>A view containing the current inventory.</returns>
    public async Task<IActionResult> Inventory()
    {
        var products = await _context.Products
            .OrderBy(p => p.StockQuantity) 
            .ToListAsync();

        return View(products);
    }

    /// <summary>
    /// Generates a printable receipt view for a specific order.
    /// Loads the order together with its customer and item details.
    /// Returns a 404 response if the order does not exist.
    /// </summary>
    /// <param name="id">The identifier of the order to display.</param>
    /// <returns>A view with order information or a NotFound result.</returns>
    public async Task<IActionResult> Receipt(int id)
    {
        var order = await _context.Orders
            .Include(o => o.User)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null) return NotFound();

        return View(order);
    }
    
    /// <summary>
    /// Manually marks an order as 'Completed'.
    /// Useful for closing fuel-only orders that do not require kitchen preparation,
    /// or for finalizing orders on behalf of the barista.
    /// </summary>
    /// <param name="id">The unique identifier of the order to complete.</param>
    /// <returns>Redirects back to the sales register (Index).</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CompleteOrder(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order != null)
        {
            order.Status = OrderStatus.Completed;
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Index));
    }
}