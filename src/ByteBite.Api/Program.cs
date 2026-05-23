using System.Text.Json.Serialization;
using ByteBite.Api.Data;
using ByteBite.Api.Filters;
using ByteBite.Api.Hubs;
using ByteBite.Application.Services;
using ByteBite.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(options =>
{
    options.Filters.Add<ApiResponseWrapperFilter>();
    options.Filters.Add<GlobalExceptionFilter>();
}).AddJsonOptions(options =>
{
    // EF导航属性循环引用时忽略，避免JSON序列化失败
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
});
builder.Services.AddOpenApi();
builder.Services.AddSignalR();

builder.Services.AddDbContext<ByteBiteDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<MerchantService>();
builder.Services.AddScoped<StoreService>();
builder.Services.AddScoped<CategoryService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<CustomerStoreService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<IndustryCategoryService>();
builder.Services.AddScoped<TemplateService>();
builder.Services.AddScoped<DiscountRuleService>();
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<DashboardService>();

builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// 启动时自动初始化种子数据
await SeedData.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
app.MapHub<OrderHub>("/hubs/order");
app.MapHub<StoreHub>("/hubs/store");

app.Run();

public partial class Program { }