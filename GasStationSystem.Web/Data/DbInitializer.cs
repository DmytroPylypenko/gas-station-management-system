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
                    Description = "Standard unleaded gasoline for everyday use.",
                    Price = 55.00m,
                    Type = ProductType.Fuel,
                    StockQuantity = 5000, 
                    ImageUrl = "https://encrypted-tbn0.gstatic.com/images?q=tbn:ANd9GcT3rC75V9i_rP_4txyL1QCMeUzBoNRcSxcjQQ&s"
                },
                new Product
                {
                    Name = "Petrol A-98 Nano",
                    Description = "Premium high-octane fuel for better performance.",
                    Price = 60.00m,
                    Type = ProductType.Fuel,
                    StockQuantity = 3000,
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/3/36/Bundesstra%C3%9Fe_98_number.svg/1200px-Bundesstra%C3%9Fe_98_number.svg.png"
                },
                new Product
                {
                    Name = "Diesel Euro",
                    Description = "High quality diesel fuel",
                    Price = 52.50m,
                    Type = ProductType.Fuel,
                    StockQuantity = 4000,
                    ImageUrl = "https://upload.wikimedia.org/wikipedia/commons/thumb/c/ca/Diesel.svg/1200px-Diesel.svg.png"
                },
                new Product
                {
                    Name = "LPG Gas",
                    Description = "Liquefied petroleum gas. Economical and eco-friendly.",
                    Price = 28.90m,
                    Type = ProductType.Fuel,
                    StockQuantity = 10000,
                    ImageUrl = "https://lpgtech.ua/uploads/images/ca5bf-logo-lpg.png"
                },
                new Product
                {
                    Name = "Cappuccino L",
                    Description = "Freshly brewed arabica with steamed milk foam.",
                    Price = 45.00m,
                    Type = ProductType.Food,
                    StockQuantity = 100,
                    ImageUrl = "https://images.unsplash.com/photo-1572442388796-11668a67e53d?auto=format&fit=crop&w=600&q=80"
                },
                new Product
                {
                    Name = "Americano",
                    Description = "Classic black coffee. Strong and energizing.",
                    Price = 35.00m,
                    Type = ProductType.Food,
                    StockQuantity = 150,
                    ImageUrl = "https://images.unsplash.com/photo-1559496417-e7f25cb247f3?auto=format&fit=crop&w=600&q=80"
                },
                new Product
                {
                    Name = "Chicken Hot Dog XL",
                    Description = "Classic hot dog with mustard",
                    Price = 65.00m,
                    Type = ProductType.Food,
                    StockQuantity = 50,
                    ImageUrl = "https://www.shutterstock.com/image-photo/sauerkraut-sweet-mustard-hot-dog-600nw-1222770619.jpg"
                },
                new Product
                {
                    Name = "Chicken Burger",
                    Description = "Juicy beef burger with cheese and fresh vegetables.",
                    Price = 120.00m,
                    Type = ProductType.Food,
                    StockQuantity = 30,
                    ImageUrl = "https://i0.wp.com/flaevor.com/wp-content/uploads/2022/04/SambalFriedChickenBurger1.jpg?resize=1024%2C830&ssl=1"
                },
                new Product
                {
                    Name = "Butter Croissant",
                    Description = "Freshly baked french pastry.",
                    Price = 40.00m,
                    Type = ProductType.Food,
                    StockQuantity = 40,
                    ImageUrl = "https://images.unsplash.com/photo-1555507036-ab1f4038808a?auto=format&fit=crop&w=600&q=80"
                }
            );
            await context.SaveChangesAsync();
        }
    }
}