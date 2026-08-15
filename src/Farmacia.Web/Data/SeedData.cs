using Farmacia.Web.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Farmacia.Web.Data;

public static class SeedData
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        await context.Database.MigrateAsync();

        await SeedRolesAndAdminAsync(services);
        SeedCatalog(context);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAndAdminAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        const string adminRole = "Admin";
        if (!await roleManager.RoleExistsAsync(adminRole))
        {
            await roleManager.CreateAsync(new IdentityRole(adminRole));
        }

        const string adminEmail = "admin@farmaciasalud.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);
        if (adminUser is null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                DisplayName = "Administrador",
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(adminUser, "Farmacia#2024!");
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, adminRole);
            }
        }
    }

    private static void SeedCatalog(ApplicationDbContext context)
    {
        if (context.Categories.Any())
        {
            return;
        }

        var categories = new List<Category>
        {
            new() { Name = "Medicamentos", Slug = "medicamentos", Icon = "bi-capsule", Description = "Medicamentos de venta libre y con receta." },
            new() { Name = "Cuidado Personal", Slug = "cuidado-personal", Icon = "bi-droplet", Description = "Higiene, dermocosmética y cuidado diario." },
            new() { Name = "Vitaminas y Suplementos", Slug = "vitaminas-suplementos", Icon = "bi-capsule-pill", Description = "Vitaminas, minerales y suplementos nutricionales." },
            new() { Name = "Cuidado del Bebé", Slug = "cuidado-bebe", Icon = "bi-emoji-smile", Description = "Productos para la salud y el cuidado de bebés." },
            new() { Name = "Equipo Médico", Slug = "equipo-medico", Icon = "bi-heart-pulse", Description = "Tensiómetros, termómetros y accesorios de salud." },
        };

        context.Categories.AddRange(categories);
        context.SaveChanges();

        var meds = categories[0];
        var cuidado = categories[1];
        var vitaminas = categories[2];
        var bebe = categories[3];
        var equipo = categories[4];

        var products = new List<Product>
        {
            new() { Name = "Paracetamol 500mg (20 tabletas)", Slug = "paracetamol-500mg", Description = "Analgésico y antipirético de uso común para dolor y fiebre.", Price = 3.50m, Stock = 120, CategoryId = meds.Id, IsFeatured = true, ImageUrl = "https://images.unsplash.com/photo-1584308666744-24d5c474f2ae?w=600" },
            new() { Name = "Ibuprofeno 400mg (30 tabletas)", Slug = "ibuprofeno-400mg", Description = "Antiinflamatorio no esteroideo para dolor e inflamación.", Price = 4.80m, Stock = 90, CategoryId = meds.Id, ImageUrl = "https://images.unsplash.com/photo-1550572017-edd951b55104?w=600" },
            new() { Name = "Amoxicilina 500mg (Requiere receta)", Slug = "amoxicilina-500mg", Description = "Antibiótico de amplio espectro. Venta bajo receta médica.", Price = 8.90m, Stock = 40, CategoryId = meds.Id, RequiresPrescription = true, ImageUrl = "https://images.unsplash.com/photo-1471864190281-a93a3070b6de?w=600" },
            new() { Name = "Loratadina 10mg (10 tabletas)", Slug = "loratadina-10mg", Description = "Antihistamínico para alergias estacionales.", Price = 5.20m, Stock = 75, CategoryId = meds.Id, ImageUrl = "https://images.unsplash.com/photo-1631549916768-4119b2e5f926?w=600" },

            new() { Name = "Alcohol en Gel 500ml", Slug = "alcohol-gel-500ml", Description = "Gel antibacterial con 70% de alcohol.", Price = 4.20m, Stock = 200, CategoryId = cuidado.Id, IsFeatured = true, ImageUrl = "https://images.unsplash.com/photo-1584744982491-665216d95f8b?w=600" },
            new() { Name = "Protector Solar FPS 50", Slug = "protector-solar-fps50", Description = "Protección solar de amplio espectro para todo tipo de piel.", Price = 12.90m, Stock = 60, CategoryId = cuidado.Id, ImageUrl = "https://images.unsplash.com/photo-1556228720-195a672e8a03?w=600" },
            new() { Name = "Jabón Dermatológico Neutro", Slug = "jabon-dermatologico-neutro", Description = "Ideal para pieles sensibles y con tendencia atópica.", Price = 6.50m, Stock = 80, CategoryId = cuidado.Id, ImageUrl = "https://images.unsplash.com/photo-1600857062241-98e5dba7f214?w=600" },

            new() { Name = "Vitamina C 1000mg (60 tabletas)", Slug = "vitamina-c-1000mg", Description = "Suplemento antioxidante para reforzar el sistema inmune.", Price = 9.90m, Stock = 100, CategoryId = vitaminas.Id, IsFeatured = true, ImageUrl = "https://images.unsplash.com/photo-1607619056574-7b8d3ee536b2?w=600" },
            new() { Name = "Complejo B (30 cápsulas)", Slug = "complejo-b-30-capsulas", Description = "Vitaminas del complejo B para energía y sistema nervioso.", Price = 8.30m, Stock = 70, CategoryId = vitaminas.Id, ImageUrl = "https://images.unsplash.com/photo-1550572017-edd951b55104?w=600" },
            new() { Name = "Omega 3 (60 cápsulas)", Slug = "omega-3-60-capsulas", Description = "Ácidos grasos esenciales para la salud cardiovascular.", Price = 14.50m, Stock = 55, CategoryId = vitaminas.Id, ImageUrl = "https://images.unsplash.com/photo-1616671276441-2f2c277b8bf6?w=600" },

            new() { Name = "Pañales Talla M (36 unidades)", Slug = "panales-talla-m", Description = "Pañales hipoalergénicos con máxima absorción.", Price = 15.90m, Stock = 45, CategoryId = bebe.Id, ImageUrl = "https://images.unsplash.com/photo-1622290291468-a28f7a7dc6a8?w=600" },
            new() { Name = "Toallitas Húmedas para Bebé", Slug = "toallitas-humedas-bebe", Description = "Toallitas suaves libres de alcohol y perfume.", Price = 3.90m, Stock = 90, CategoryId = bebe.Id, ImageUrl = "https://images.unsplash.com/photo-1584582397587-1a4544cee7be?w=600" },

            new() { Name = "Tensiómetro Digital de Brazo", Slug = "tensiometro-digital-brazo", Description = "Medidor de presión arterial digital de fácil uso.", Price = 32.00m, Stock = 25, CategoryId = equipo.Id, IsFeatured = true, ImageUrl = "https://images.unsplash.com/photo-1631815589968-fdb09a223b1e?w=600" },
            new() { Name = "Termómetro Digital Infrarrojo", Slug = "termometro-digital-infrarrojo", Description = "Medición de temperatura sin contacto en segundos.", Price = 18.50m, Stock = 35, CategoryId = equipo.Id, ImageUrl = "https://images.unsplash.com/photo-1584362917165-526a968579e8?w=600" },
            new() { Name = "Oxímetro de Pulso", Slug = "oximetro-de-pulso", Description = "Mide la saturación de oxígeno y frecuencia cardíaca.", Price = 22.90m, Stock = 30, CategoryId = equipo.Id, ImageUrl = "https://images.unsplash.com/photo-1583912267550-d6c2ac3196c0?w=600" },
        };

        context.Products.AddRange(products);
    }
}
