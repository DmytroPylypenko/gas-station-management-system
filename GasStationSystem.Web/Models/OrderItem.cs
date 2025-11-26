using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GasStationSystem.Web.Models;

/// <summary>
/// Represents a specific line item within an Order.
/// Links a Product to an Order and captures the price at the moment of purchase.
/// </summary>
public class OrderItem
{
    [Key]
    public int Id { get; set; }
    
    public int OrderId { get; set; }
    [ForeignKey("OrderId")]
    public Order Order { get; set; } = null!;
    
    public int ProductId { get; set; }
    [ForeignKey("ProductId")]
    public Product Product { get; set; } = null!;

    [Range(0.01, 10000)]
    public double Quantity { get; set; } 
    
    // We must store the price at the moment of purchase.
    // If the product price changes later, historical data remains accurate.
    [Column(TypeName = "decimal(18,2)")]
    public decimal PriceAtMoment { get; set; }
}