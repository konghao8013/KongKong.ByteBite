using ByteBite.Application.DTOs.Discount;
using ByteBite.Application.DTOs.Merchant;
using ByteBite.Application.DTOs.Store;
using ByteBite.Application.DTOs.Common;
using TestDataGenerator = ByteBite.IntegrationTests.Helpers.TestDataGenerator;

namespace ByteBite.IntegrationTests.Modules;

/// <summary>
/// 优惠活动模块集成测试 - 验证优惠活动的增删改查及状态切换
/// </summary>
public class DiscountRulesIntegrationTests : IClassFixture<ByteBiteWebAppFactory>
{
    private readonly HttpClient _client;

    public DiscountRulesIntegrationTests(ByteBiteWebAppFactory factory)
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
    /// 创建优惠规则并返回其DTO
    /// </summary>
    private async Task<DiscountRuleDto> CreateDiscountRuleAsync(Guid storeId)
    {
        var request = TestDataGenerator.CreateDiscountRuleRequest(storeId);
        var response = await _client.PostAsJsonAsync($"/api/stores/{storeId}/discount-rules", request);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DiscountRuleDto>>();
        return result!.Data!;
    }

    /// <summary>
    /// 测试：创建优惠活动 - 有效请求返回创建结果，验证名称和状态
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreated()
    {
        var merchantId = await CreateTestMerchantAsync();
        var storeId = await CreateTestStoreAsync(merchantId);
        var request = TestDataGenerator.CreateDiscountRuleRequest(storeId);

        var response = await _client.PostAsJsonAsync($"/api/stores/{storeId}/discount-rules", request);

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DiscountRuleDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(request.Name);
        result.Data.Status.Should().Be("active");
    }

    /// <summary>
    /// 测试：获取店铺优惠活动列表 - 创建规则后查询应至少包含1条
    /// </summary>
    [Fact]
    public async Task GetByStoreIdAsync_ReturnsList()
    {
        var merchantId = await CreateTestMerchantAsync();
        var storeId = await CreateTestStoreAsync(merchantId);
        await CreateDiscountRuleAsync(storeId);

        var response = await _client.GetAsync($"/api/stores/{storeId}/discount-rules");

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DiscountRuleDto>>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// 测试：切换优惠活动状态 - 从active切换为inactive
    /// </summary>
    [Fact]
    public async Task ToggleStatusAsync_ActiveToInactive()
    {
        var merchantId = await CreateTestMerchantAsync();
        var storeId = await CreateTestStoreAsync(merchantId);
        var rule = await CreateDiscountRuleAsync(storeId);

        var response = await _client.PatchAsync($"/api/discount-rules/{rule.Id}/toggle-status", null);

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DiscountRuleDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be("inactive");
    }

    /// <summary>
    /// 测试：删除优惠活动 - 软删除后列表中不应再包含该规则
    /// </summary>
    [Fact]
    public async Task DeleteAsync_SoftDeletes()
    {
        var merchantId = await CreateTestMerchantAsync();
        var storeId = await CreateTestStoreAsync(merchantId);
        var rule = await CreateDiscountRuleAsync(storeId);

        var deleteResponse = await _client.DeleteAsync($"/api/discount-rules/{rule.Id}");
        deleteResponse.Should().BeSuccessful();

        var listResponse = await _client.GetAsync($"/api/stores/{storeId}/discount-rules");
        listResponse.Should().BeSuccessful();
        var listResult = await listResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<DiscountRuleDto>>>();
        listResult.Should().NotBeNull();
        listResult!.Data.Should().NotBeNull();
        listResult.Data!.Should().NotContain(r => r.Id == rule.Id);
    }
}
