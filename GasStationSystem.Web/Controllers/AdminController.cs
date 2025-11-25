using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using GasStationSystem.Web.Data;
using GasStationSystem.Web.Models;

namespace GasStationSystem.Web.Controllers;

/// <summary>
/// Provides administrative functionality such as monitoring system activity,
/// managing products, and assigning roles to users. Accessible only to Admins.
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;

    public AdminController(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Displays administrative dashboard metrics such as revenue,
    /// total orders, user count, and low-stock notifications.
    /// Also shows a list of the five most recent orders.
    /// </summary>
    /// <returns>A dashboard view with system statistics.</returns>
    public async Task<IActionResult> Index()
    {
        ViewBag.TotalRevenue = await _context.Orders.SumAsync(o => o.TotalAmount);
        ViewBag.TotalOrders = await _context.Orders.CountAsync();
        ViewBag.TotalUsers = await _userManager.Users.CountAsync();
        ViewBag.LowStockCount = await _context.Products.CountAsync(p => p.StockQuantity < 50);

        var recentOrders = await _context.Orders
            .Include(o => o.User)
            .OrderByDescending(o => o.CreatedAt)
            .Take(5) 
            .ToListAsync();
        
        return View(recentOrders);
    }

    /// <summary>
    /// Displays a list of all products available in the system.
    /// </summary>
    public async Task<IActionResult> Products()
    {
        return View(await _context.Products.ToListAsync());
    }

    /// <summary>
    /// Renders a form for creating a new product or editing an existing one.
    /// </summary>
    /// <param name="id">The identifier of the product to edit, or null to create a new product.</param>
    /// <returns>A product form view.</returns>
    public async Task<IActionResult> Upsert(int? id)
    {
        if (id == null || id == 0)
        {
            return View(new Product());
        }
        
        Product? product = await _context.Products.FindAsync(id);
        
        if (product == null)
        {
            return NotFound();
        }
        
        return View(product);
    }

    /// <summary>
    /// Creates a new product or updates an existing one based on the submitted form data.
    /// </summary>
    /// <param name="product">The product model containing form data.</param>
    /// <returns>A redirect to the product list on success, or the form view on validation failure.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upsert(Product product)
    {
        if (ModelState.IsValid)
        {
            if (product.Id == 0) _context.Products.Add(product);
            else _context.Products.Update(product);
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Products));
        }
        return View(product);
    }

    /// <summary>
    /// Deletes a product by its identifier.
    /// </summary>
    /// <param name="id">The id of the product to delete.</param>
    [HttpPost] 
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Products));
    }
    
    /// <summary>
    /// Displays a list of all registered users along with their assigned roles.
    /// </summary>
    /// <returns>A view containing users and their roles.</returns>
    public async Task<IActionResult> Users()
    {
        var users = await _userManager.Users.ToListAsync();
        var userViewModels = new List<UserWithRolesViewModel>();

        foreach (var user in users)
        {
            var thisViewModel = new UserWithRolesViewModel
            {
                UserId = user.Id,
                Email = user.Email,
                Roles = (await _userManager.GetRolesAsync(user)).ToList()
            };
            userViewModels.Add(thisViewModel);
        }

        return View(userViewModels);
    }

    /// <summary>
    /// Displays role management page for a specific user.
    /// Shows assigned roles and all available roles.
    /// </summary>
    /// <param name="userId">The identifier of the user to manage.</param>
    /// <returns>A view with role assignment options.</returns>
    public async Task<IActionResult> ManageUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        var model = new UserWithRolesViewModel
        {
            UserId = user.Id,
            Email = user.Email,
            Roles = (await _userManager.GetRolesAsync(user)).ToList(),
            AllRoles = _roleManager.Roles.Select(r => r.Name).ToList()
        };

        return View(model);
    }

    /// <summary>
    /// Updates the roles assigned to a selected user.
    /// Removes existing roles and assigns newly selected ones.
    /// </summary>
    /// <param name="userId">The user's identifier.</param>
    /// <param name="roles">A list of selected role names.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ManageUser(string userId, List<string> roles)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        // 1. Delete old roles
        var userRoles = await _userManager.GetRolesAsync(user);
        await _userManager.RemoveFromRolesAsync(user, userRoles);

        // 2. Add new roles
        await _userManager.AddToRolesAsync(user, roles);

        return RedirectToAction(nameof(Users));
    }

    /// <summary>
    /// Permanently deletes a user from the system.
    /// </summary>
    /// <param name="userId">The ID of the user to delete.</param>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user != null)
        {
            await _userManager.DeleteAsync(user);
        }
        return RedirectToAction(nameof(Users));
    }
}