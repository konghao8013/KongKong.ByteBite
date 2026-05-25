using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Api.Data;

public class IndustryCategorySeeder : IDataSeeder
{
    public int Order => 20;

    public async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ByteBiteDbContext>();

        if (await db.IndustryCategories.AnyAsync()) return;

        var cat1 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000001"), Name = "餐饮", Level = 1, SortOrder = 1, Icon = "🍜", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var cat2 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000002"), Parent = cat1, Name = "烧烤", Level = 2, SortOrder = 1, Icon = "🔥", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var cat3 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000003"), Parent = cat2, Name = "重庆特色烧烤", Level = 3, SortOrder = 1, Icon = "🌶️", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var cat4 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000004"), Parent = cat2, Name = "东北烧烤", Level = 3, SortOrder = 2, Icon = "🥩", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var cat5 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000005"), Parent = cat1, Name = "小吃快餐", Level = 2, SortOrder = 2, Icon = "🍔", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        var cat6 = new IndustryCategory { Id = Guid.Parse("a0000000-0000-0000-0000-000000000006"), Parent = cat1, Name = "饮品甜点", Level = 2, SortOrder = 3, Icon = "🧋", IsVisible = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow };
        db.IndustryCategories.AddRange(cat1, cat2, cat3, cat4, cat5, cat6);
        await db.SaveChangesAsync();
    }
}
