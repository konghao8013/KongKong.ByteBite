using ByteBite.Infrastructure.Persistence;
using ByteBite.Infrastructure.Persistence.Entities;
using ByteBite.Shared.Helpers;
using Microsoft.EntityFrameworkCore;

namespace ByteBite.Api.Data;

public class StoreCodeSeeder : IDataSeeder
{
    public int Order => 100;

    public async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ByteBiteDbContext>();

        var storesWithoutCode = await db.Stores.Where(s => string.IsNullOrEmpty(s.StoreCode)).OrderBy(s => s.CreatedAt).ToListAsync();
        if (storesWithoutCode.Count == 0) return;

        var existingCount = await db.Stores.CountAsync(s => !string.IsNullOrEmpty(s.StoreCode));
        for (var i = 0; i < storesWithoutCode.Count; i++)
        {
            storesWithoutCode[i].StoreCode = Base36Encoder.Encode(existingCount + i + 1);
        }
        await db.SaveChangesAsync();
    }
}
