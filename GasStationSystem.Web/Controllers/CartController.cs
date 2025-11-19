using GasStationSystem.Web.Data;
using GasStationSystem.Web.Infrastructure.Extensions;
using GasStationSystem.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace GasStationSystem.Web.Controllers;

public class CartController : Controller
{
    private readonly ApplicationDbContext _context;
    private const string CartSessionKey = "CartSession";

    public CartController(ApplicationDbContext context)
    {
        _context = context;
    }

    public IActionResult Index()
    {
        var cart = HttpContext.Session.GetObject<CartViewModel>(CartSessionKey) ?? new CartViewModel();
        return View(cart);
    }

    [HttpPost]
    public async Task<IActionResult> AddToCart(int id)
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
            cartItem.Quantity++;
        }
        else
        {
            cart.Items.Add(new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                ImageUrl = product.ImageUrl ?? "https://placehold.co/100",
                Quantity = 1
            });
        }

        HttpContext.Session.SetObject(CartSessionKey, cart);

        return Json(new { success = true, message = $"{product.Name} added to cart", count = cart.Items.Sum(i => i.Quantity) });
    }

    
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
    
    public IActionResult Clear()
    {
        HttpContext.Session.Remove(CartSessionKey);
        return RedirectToAction("Index");
    }
}