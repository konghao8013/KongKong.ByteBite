namespace ByteBite.Api.Data;

public interface IDataSeeder
{
    int Order { get; }
    Task SeedAsync(IServiceProvider services);
}
