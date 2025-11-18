using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GasStationSystem.Web.Models;

public enum ProductType
{
    Fuel,  
    Food,  
    Service 
}

public class Product
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Product name is required")]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    [Column(TypeName = "decimal(18,2)")] 
    [Range(0, 100000, ErrorMessage = "Price must be greater than 0")]
    public decimal Price { get; set; }

    [Required]
    public ProductType Type { get; set; }
    
    // Quantity in stock: Liters for fuel, Units for food
    [Range(0, double.MaxValue)]
    public double StockQuantity { get; set; } 

    [StringLength(2048)]
    public string? ImageUrl { get; set; }
}