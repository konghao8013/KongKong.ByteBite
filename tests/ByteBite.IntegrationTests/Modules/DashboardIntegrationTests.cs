using ByteBite.Application.DTOs.Dashboard;
using ByteBite.Application.DTOs.Merchant;
using ByteBite.Application.DTOs.Store;
using ByteBite.Application.DTOs.Common;
using TestDataGenerator = ByteBite.IntegrationTests.Helpers.TestDataGenerator;

namespace ByteBite.IntegrationTests.Modules;

/// <summary>
/// 经营数据看板模块集成测试 - 验证概览、商品销售、营收趋势等接口
/// </summary>
public class DashboardIntegrationTests : IClassFixture<ByteBiteWebAppFactory>
{
    private readonly HttpClient _client;

    public DashboardIntegrationTests(ByteBiteWebAppFactory factory)
    {
        _client = factory.CreateClientWithNoAuth();
    }

    /// <summary>
    /// 创建测试商家并返回其ID
    /// </summary>
    private async Task<Guid> CreateTestMerchantAsync()
    {
        var request = TestDataGenerator.CreateMerchantRegisterRequest();
        var response = await _client.PostAsJsonAsync("/api/merchants/register", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MerchantDto>>();
        return result!.Data!.Id;
    }

    /// <summary>
    /// 创建测试店铺并返回其ID
    /// </summary>
    private async Task<Guid> CreateTestStoreAsync(Guid merchantId)
    {
        var request = TestDataGenerator.CreateStoreRequest(merchantId);
        var response = await _client.PostAsJsonAsync("/api/stores", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<StoreDto>>();
        return result!.Data!.Id;
    }

    /// <summary>
    /// 测试：获取经营数据概览 - 应返回概览数据
    /// </summary>
    [Fact]
    public async Task GetOverviewAsync_ReturnsOverview()
    {
        var merchantId = await CreateTestMerchantAsync();
        var storeId = await CreateTestStoreAsync(merchantId);

        var response = await _client.GetAsync($"/api/dashboard/{storeId}/overview");

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DashboardOverviewDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
    }

    /// <summary>
    /// 测试：获取商品销售分析 - 应返回销售列表
    /// </summary>
    [Fact]
    public async Task GetProductSalesAsync_ReturnsList()
    {
        var merchantId = await CreateTestMerchantAsync();
        var storeId = await CreateTestStoreAsync(merchantId);

        var response = await _client.GetAsync($"/api/dashboard/{storeId}/product-sales");

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductSalesDto>>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
    }

    /// <summary>
    /// 测试：获取营收趋势 - 应返回趋势数据
    /// </summary>
    [Fact]
    public async Task GetRevenueTrendAsync_ReturnsTrend()
    {
        var merchantId = await CreateTestMerchantAsync();
        var storeId = await CreateTestStoreAsync(merchantId);

        var response = await _client.GetAsync($"/api/dashboard/{storeId}/revenue-trend?timeRange=last7days");

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<TrendDataDto>>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
    }
}
