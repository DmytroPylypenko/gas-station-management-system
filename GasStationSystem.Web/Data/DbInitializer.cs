using GasStationSystem.Web.Models;
using Microsoft.AspNetCore.Identity;

namespace GasStationSystem.Web.Data;

public class DbInitializer
{
    public static async Task Initialize(IServiceProvider serviceProvider)
    {
        // Get necessary services
        var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

        // 1. Ensure the database is created
        await context.Database.EnsureCreatedAsync();

        // 2. Create Roles if they don't exist
        string[] roleNames = { "Admin", "Cashier", "User", "Barista" };
        foreach (var roleName in roleNames)
        {
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole(roleName));
            }
        }

        // 3. Create Default Admin User
        var adminEmail = "admin@gas.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            adminUser = new IdentityUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };
            
            var result = await userManager.CreateAsync(adminUser, "Admin123!");
            
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, "Admin");
            }
        }
        
        // 4. Create Default Cashier User
        var cashierEmail = "cashier@gas.com";
        if (await userManager.FindByEmailAsync(cashierEmail) == null)
        {
            var cashier = new IdentityUser
            {
                UserName = cashierEmail,
                Email = cashierEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(cashier, "Cashier123!");
            await userManager.AddToRoleAsync(cashier, "Cashier");
        }

        // 5. Create Default Barista User
        var baristaEmail = "barista@gas.com";
        if (await userManager.FindByEmailAsync(baristaEmail) == null)
        {
            var barista = new IdentityUser
            {
                UserName = baristaEmail,
                Email = baristaEmail,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(barista, "Barista123!"); 
            await userManager.AddToRoleAsync(barista, "Barista");
        }
        
        // 6. Seed Products (if empty)
        if (!context.Products.Any())
        {
            context.Products.AddRange(
                new Product
                {
                    Name = "Petrol A-95",
                    Description = "Premium unleaded gasoline",
                    Price = 55.00m,
                    Type = ProductType.Fuel,
                    StockQuantity = 5000, // Liters
                    ImageUrl = "https://placehold.co/600x400?text=Fuel+A95"
                },
                new Product
                {
                    Name = "Diesel Euro",
                    Description = "High quality diesel fuel",
                    Price = 52.50m,
                    Type = ProductType.Fuel,
                    StockQuantity = 4000
                },
                new Product
                {
                    Name = "Cappuccino L",
                    Description = "Freshly brewed coffee with milk",
                    Price = 45.00m,
                    Type = ProductType.Food,
                    StockQuantity = 100 // Cups
                },
                new Product
                {
                    Name = "Hot Dog XL",
                    Description = "Classic hot dog with mustard",
                    Price = 65.00m,
                    Type = ProductType.Food,
                    StockQuantity = 50
                }
            );
            await context.SaveChangesAsync();
        }
    }
}