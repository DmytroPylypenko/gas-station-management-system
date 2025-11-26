using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace GasStationSystem.Web.Models;

public enum OrderStatus
{
    New,        
    Processing, 
    Completed,  
    Cancelled   
}

/// <summary>
/// Represents a finalized sales transaction stored in the database.
/// Contains information about the customer, total amount, and status.
/// </summary>
public class Order
{
    [Key]
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.New;
    
    // Link to the User (Identity)
    [StringLength(450)]
    public string? UserId { get; set; }
    [ForeignKey("UserId")]
    public IdentityUser? User { get; set; }
    
    // Navigation property for order items
    public List<OrderItem> OrderItems { get; set; } = new();
}