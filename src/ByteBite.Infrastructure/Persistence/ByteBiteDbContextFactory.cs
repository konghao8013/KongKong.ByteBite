using ByteBite.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace ByteBite.Infrastructure;

public class ByteBiteDbContextFactory : IDesignTimeDbContextFactory<ByteBiteDbContext>
{
    public ByteBiteDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<ByteBiteDbContext>();
        optionsBuilder.UseNpgsql("Host=192.168.3.22;Port=5432;Database=kongkong_bytebite;Username=konghao;Password=hitek.123");
        return new ByteBiteDbContext(optionsBuilder.Options);
    }
}
