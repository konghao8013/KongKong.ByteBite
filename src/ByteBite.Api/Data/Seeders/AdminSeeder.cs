using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Api.Data;

public class AdminSeeder : IDataSeeder
{
    public int Order => 10;

    public async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ByteBiteDbContext>();

        var admin = await db.Admins.FirstOrDefaultAsync(a => a.Username == "admin");
        if (admin == null)
        {
            db.Admins.Add(new Admin
            {
                Id = Guid.NewGuid(), Username = "admin", PasswordHash = PasswordHasher.HashPassword("admin123"),
                DisplayName = "系统管理员", Role = "super_admin", Status = "active",
                CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            });
            await db.SaveChangesAsync();
        }
        else if (!PasswordHasher.VerifyPassword("admin123", admin.PasswordHash))
        {
            admin.PasswordHash = PasswordHasher.HashPassword("admin123");
            admin.Status = "active";
            await db.SaveChangesAsync();
        }
    }
}
