namespace GasStationSystem.Web.Models;

/// <summary>
/// ViewModel representing the aggregate state of the user's shopping cart.
/// Used to pass cart data from the controller to the view.
/// </summary>
public class CartViewModel
{
    public List<CartItem> Items { get; set; } = new();
    public decimal GrandTotal => Items.Sum(i => i.Total);
}