using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using GasStationSystem.Web.Controllers;
using GasStationSystem.Web.Data;
using GasStationSystem.Web.Models;
using System.Security.Claims;
using System.Text.Json;
using GasStationSystem.Tests.Infrastructure;
using Xunit;

namespace GasStationSystem.Tests.Controllers;

public class CartControllerTests
{
    /// <summary>
    /// Creates a fresh in-memory database for each test.
    /// This prevents data leaking between tests and ensures full isolation.
    /// </summary>
    private ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    /// <summary>
    /// Builds a CartController instance.
    /// This helper centralizes all controller test setup.
    /// </summary>
    private CartController CreateController(ApplicationDbContext context, CartViewModel? cart = null)
    {
        var controller = new CartController(context);
        var session = new FakeSession();

        // Preload a cart into session when needed
        if (cart != null)
        {
            var json = JsonSerializer.Serialize(cart);
            session.Set("CartSession", System.Text.Encoding.UTF8.GetBytes(json));
        }

        // Build HttpContext with session and authenticated user
        var httpContext = new DefaultHttpContext
        {
            Session = session
        };

        var identity = new ClaimsIdentity([
            new Claim(ClaimTypes.NameIdentifier, "test-user-id")
        ], "TestAuth");

        httpContext.User = new ClaimsPrincipal(identity);

        controller.ControllerContext = new ControllerContext
        {
            HttpContext = httpContext
        };

        return controller;
    }
    
    [Fact]
    public void Index_ReturnsView_WithCartFromSession()
    {
        // Arrange
        var context = CreateDb();
        var cart = new CartViewModel
        {
            Items = { new CartItem { ProductId = 1, ProductName = "Fuel", Quantity = 2, Price = 10 } }
        };

        var controller = CreateController(context, cart);

        // Act
        var result = controller.Index();

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<CartViewModel>(view.Model);
        Assert.Single(model.Items);
    }

    [Fact]
    public async Task AddToCart_AddsNewItem_WhenNotExisting()
    {
        // Arrange
        var context = CreateDb();
        context.Products.Add(new Product { Id = 1, Name = "Fuel", Price = 50 });
        await context.SaveChangesAsync();

        var controller = CreateController(context, new CartViewModel());

        // Act
        var result = await controller.AddToCart(1);

        // Assert
        var json = Assert.IsType<JsonResult>(result);
        var value = json.Value!;

        // Expose properties through reflection
        var success = (bool)value.GetType().GetProperty("success")!.GetValue(value)!;
        var count = (int)value.GetType().GetProperty("count")!.GetValue(value)!;
        
        Assert.True(success);
        Assert.Equal(1, count);
    }

    [Fact]
    public async Task AddToCart_IncrementsQuantity_WhenItemExists()
    {
        // Arrange
        var context = CreateDb();
        context.Products.Add(new Product { Id = 1, Name = "Fuel", Price = 50 });
        await context.SaveChangesAsync();

        var cart = new CartViewModel
        {
            Items = { new CartItem { ProductId = 1, ProductName = "Fuel", Price = 50, Quantity = 1 } }
        };

        var controller = CreateController(context, cart);

        // Act
        await controller.AddToCart(1, quantity: 2);

        var updatedBytes = controller.HttpContext.Session.Get("CartSession");
        var updatedCart = JsonSerializer.Deserialize<CartViewModel>(updatedBytes)!;
        
        // Assert
        Assert.Equal(3, updatedCart.Items[0].Quantity);
    }

    [Fact]
    public void Remove_RemovesItem_WhenExists()
    {
        // Arrange
        var context = CreateDb();
        var cart = new CartViewModel
        {
            Items = { new CartItem { ProductId = 1, Quantity = 1 } }
        };

        var controller = CreateController(context, cart);

        // Act
        var result = controller.Remove(1);

        var updatedBytes = controller.HttpContext.Session.Get("CartSession");
        var updatedCart = JsonSerializer.Deserialize<CartViewModel>(updatedBytes)!;
        
        // Assert
        Assert.Empty(updatedCart.Items);
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public void Clear_RemovesSessionKey()
    {
        // Arrange
        var context = CreateDb();
        var controller = CreateController(context, new CartViewModel());

        // Act
        var result = controller.Clear();

        // Assert
        Assert.IsType<RedirectToActionResult>(result);
    }

    [Fact]
    public async Task Checkout_ReturnsIndex_WhenCartIsEmpty()
    {
        // Arrange
        var context = CreateDb();
        var controller = CreateController(context, new CartViewModel());

        // Act
        var result = await controller.Checkout();

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }
    
    [Fact]
    public async Task Checkout_CreatesOrder_AndRedirectsToSuccess()
    {
        // Arrange
        var context = CreateDb();

        // Prepare DB with product
        context.Products.Add(new Product { Id = 1, Name = "Fuel", Price = 50, StockQuantity = 10 });
        await context.SaveChangesAsync();

        var cart = new CartViewModel
        {
            Items = { new CartItem { ProductId = 1, ProductName = "Fuel", Price = 50, Quantity = 2 } }
        };

        var controller = CreateController(context, cart);

        // Act
        var result = await controller.Checkout();

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Success", redirect.ActionName);

        // Order correctness
        var order = context.Orders.Include(o => o.OrderItems).First();
        Assert.Equal("test-user-id", order.UserId);
        Assert.Equal(100, order.TotalAmount);
        Assert.Single(order.OrderItems);

        // Stock updated
        var product = await context.Products.FirstAsync();
        Assert.Equal(8, product.StockQuantity); // 10 - 2
    }
    
    [Fact]
    public void Success_ReturnsView_WithOrderId()
    {
        // Arrange
        var context = CreateDb();
        var controller = CreateController(context);

        // Act
        var result = controller.Success(123);

        // Assert
        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal(123, view.Model);
    }
}
