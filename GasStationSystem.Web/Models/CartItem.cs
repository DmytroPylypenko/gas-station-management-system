namespace GasStationSystem.Web.Models;

/// <summary>
/// Represents a temporary item in the user's shopping cart session.
/// Not stored in the database until checkout.
/// </summary>
public class CartItem
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ImageUrl { get; set; } = string.Empty;

    public decimal Total => Price * Quantity;
}