using ByteBite.Api;
using ByteBite.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ByteBite.IntegrationTests;

/// <summary>
/// 集成测试WebApplication工厂 - 使用真实数据库进行端到端测试
/// </summary>
public class ByteBiteWebAppFactory : WebApplicationFactory<Program>
{
    /// <summary>测试数据库连接字符串</summary>
    private const string TestConnectionString =
        "Host=192.168.3.22;Port=5432;Database=kongkong_bytebite;Username=konghao;Password=hitek.123";

    /// <summary>
    /// 配置测试主机 - 使用真实PostgreSQL数据库
    /// </summary>
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 替换DbContext为测试数据库连接
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ByteBiteDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<ByteBiteDbContext>(options =>
            {
                options.UseNpgsql(TestConnectionString);
            });
        });
    }

    /// <summary>
    /// 创建HttpClient用于发送API请求
    /// </summary>
    public HttpClient CreateClientWithNoAuth()
    {
        return CreateClient();
    }
}