using GasStationSystem.Web.Data;
using GasStationSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GasStationSystem.Web.Controllers;

/// <summary>
/// Manages the Barista dashboard for processing food-related orders.
/// Accessible only to users with 'Barista' or 'Admin' roles.
/// </summary>
[Authorize(Roles = "Barista,Admin")]
public class BaristaController : Controller
{
    private readonly ApplicationDbContext _context;

    public BaristaController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    /// <summary>
    /// Displays a queue of active orders that contain food items.
    /// Filters orders by status (New or Processing) and ensures they contain at least one Food product.
    /// </summary>
    /// <returns>A view displaying the list of orders to be prepared.</returns>
    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Processing)
            .Where(o => o.OrderItems.Any(oi => oi.Product.Type == ProductType.Food))
            .OrderBy(o => o.CreatedAt) 
            .ToListAsync();

        return View(orders);
    }
    
    /// <summary>
    /// Updates the status of a specific order (e.g., moving it to 'Processing' or 'Completed').
    /// </summary>
    /// <param name="orderId">The unique identifier of the order.</param>
    /// <param name="newStatus">The new status to set.</param>
    /// <returns>Redirects to the order queue.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateStatus(int orderId, OrderStatus newStatus)
    {
        var order = await _context.Orders.FindAsync(orderId);
        
        if (order != null)
        {
            order.Status = newStatus;
            await _context.SaveChangesAsync();
        }
        
        return RedirectToAction(nameof(Index));
    }
}