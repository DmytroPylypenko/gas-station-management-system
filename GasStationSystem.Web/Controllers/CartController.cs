using System.Security.Claims;
using GasStationSystem.Web.Data;
using GasStationSystem.Web.Infrastructure.Extensions;
using GasStationSystem.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GasStationSystem.Web.Controllers;

/// <summary>
/// Manages shopping cart operations including adding, removing items, and processing checkout.
/// Uses Session state to temporarily store cart data before the order is finalized.
/// </summary>
public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string CartSessionKey = "CartSession";

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Displays the current contents of the shopping cart.
    /// Retrieves the cart from the session or creates a new one if it doesn't exist.
    /// </summary>
    /// <returns>The Cart Index view with the current <see cref="CartViewModel"/>.</returns>
    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObject<CartViewModel>(CartSessionKey) ?? new CartViewModel();
        return View(cart);
    }

    /// <summary>
    /// Adds a product to the cart or updates the quantity if it already exists.
    /// Designed to be called via AJAX.
    /// </summary>
    /// <param name="id">The unique identifier of the product to add.</param>
    /// <param name="quantity">The quantity (or liters) to add. Defaults to 1.</param>
    /// <returns>A JSON result indicating success and the updated unique item count.</returns>
    [HttpPost]
    public async Task<IActionResult> AddToCart(int id, int quantity = 1)
    {
        var product = await _context.Products.FindAsync(id);
        if (product == null)
        {
            return NotFound(new { success = false, message = "Product not found" });
        }

        var cart = HttpContext.Session.GetObject<CartViewModel>(CartSessionKey) ?? new CartViewModel();

        var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == id);

        if (cartItem != null)
        {
            cartItem.Quantity += quantity;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                ImageUrl = product.ImageUrl ?? "https://placehold.co/100",
                Quantity = quantity
            });
        }

        HttpContext.Session.SetObject(CartSessionKey, cart);

        return Json(new { success = true, message = $"{quantity} x {product.Name} added to cart", count = cart.Items.Count });
    }

    /// <summary>
    /// Removes a specific item from the cart based on the product ID.
    /// </summary>
    /// <param name="id">The ID of the product to remove.</param>
    /// <returns>Redirects back to the Cart Index view.</returns>
    public IActionResult Remove(int id)
    {
        var cart = HttpContext.Session.GetObject<CartViewModel>(CartSessionKey);
        if (cart != null)
        {
            var item = cart.Items.FirstOrDefault(i => i.ProductId == id);
            if (item != null)
            {
                cart.Items.Remove(item);
                HttpContext.Session.SetObject(CartSessionKey, cart);
            }
        }
        return RedirectToAction("Index");
    }
    
    /// <summary>
    /// Clears all items from the shopping cart by removing the session key.
    /// </summary>
    /// <returns>Redirects back to the Cart Index view.</returns>
    public IActionResult Clear()
    {
        HttpContext.Session.Remove(CartSessionKey);
        return RedirectToAction("Index");
    }
    
    /// <summary>
    /// Finalizes the order process. Converts the session cart into a database Order record.
    /// Requires the user to be authorized (logged in).
    /// Updates product stock levels and clears the cart upon success.
    /// </summary>
    /// <returns>Redirects to the Success page with the Order ID, or back to Index if cart is empty.</returns>
    [Authorize] 
    public async Task<IActionResult> Checkout()
    {
        var cart = HttpContext.Session.GetObject<CartViewModel>(CartSessionKey);
        
        if (cart == null || !cart.Items.Any())
        {
            return RedirectToAction("Index");
        }
        
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        
        var order = new Order
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            Status = OrderStatus.New,
            TotalAmount = cart.GrandTotal,
            OrderItems = new List<OrderItem>()
        };
        
        foreach (var item in cart.Items)
        {
            var orderItem = new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                PriceAtMoment = item.Price 
            };
            order.OrderItems.Add(orderItem);
            
            // Decrease stock quantity
            var productDb = await _context.Products.FindAsync(item.ProductId);
            if (productDb != null) productDb.StockQuantity -= item.Quantity;
        }
        
        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        
        HttpContext.Session.Remove(CartSessionKey);
        
        return RedirectToAction("Success", new { orderId = order.Id });
    }
    
    /// <summary>
    /// Displays the order confirmation page.
    /// </summary>
    /// <param name="orderId">The ID of the successfully created order.</param>
    /// <returns>The Success view.</returns>
    public IActionResult Success(int orderId)
    {
        return View(orderId); 
    }
}