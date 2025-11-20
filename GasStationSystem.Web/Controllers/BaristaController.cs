using GasStationSystem.Web.Data;
using GasStationSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GasStationSystem.Web.Controllers;

[Authorize(Roles = "Barista,Admin")]
public class BaristaController : Controller
{
    private readonly ApplicationDbContext _context;

    public BaristaController(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IActionResult> Index()
    {
        var orders = await _context.Orders
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .Where(o => o.Status == OrderStatus.New || o.Status == OrderStatus.Processing)
            .OrderBy(o => o.CreatedAt) 
            .ToListAsync();

        return View(orders);
    }
    
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