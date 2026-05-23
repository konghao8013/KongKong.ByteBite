using ByteBite.Application.DTOs.Admin;
using ByteBite.Application.DTOs.Common;
using TestDataGenerator = ByteBite.IntegrationTests.Helpers.TestDataGenerator;

namespace ByteBite.IntegrationTests.Modules;

/// <summary>
/// 管理员模块集成测试 - 验证管理员登录、系统配置、商家列表及平台统计
/// </summary>
public class AdminIntegrationTests : IClassFixture<ByteBiteWebAppFactory>
{
    private readonly HttpClient _client;

    public AdminIntegrationTests(ByteBiteWebAppFactory factory)
    {
        _client = factory.CreateClientWithNoAuth();
    }

    /// <summary>
    /// 测试：管理员登录 - 正确凭据应返回管理员信息
    /// </summary>
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAdmin()
    {
        var request = TestDataGenerator.CreateAdminLoginRequest();

        var response = await _client.PostAsJsonAsync("/api/admin/login", request);

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AdminDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Username.Should().Be("super_admin");
    }

    /// <summary>
    /// 测试：管理员登录 - 错误密码应返回401且Data为null
    /// </summary>
    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        var request = TestDataGenerator.CreateAdminLoginRequest(password: "wrong_password");

        var response = await _client.PostAsJsonAsync("/api/admin/login", request);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Unauthorized);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<AdminDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(401);
        result.Data.Should().BeNull();
    }

    /// <summary>
    /// 测试：获取公开系统配置 - 应返回配置列表
    /// </summary>
    [Fact]
    public async Task GetPublicConfigsAsync_ReturnsConfigs()
    {
        var response = await _client.GetAsync("/api/admin/configs/public");

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<SystemConfigDto>>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Should().NotBeEmpty();
    }

    /// <summary>
    /// 测试：获取商家列表 - 应返回分页结果
    /// </summary>
    [Fact]
    public async Task GetMerchantsAsync_ReturnsList()
    {
        var response = await _client.GetAsync("/api/admin/merchants");

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<MerchantManageDto>>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
    }

    /// <summary>
    /// 测试：获取平台统计数据 - 应返回统计信息
    /// </summary>
    [Fact]
    public async Task GetPlatformStatsAsync_ReturnsStats()
    {
        var response = await _client.GetAsync("/api/admin/stats");

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PlatformStatsDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
    }
}
