using System.Net;
using System.Net.Http.Json;
using ByteBite.Application.DTOs.Merchant;
using ByteBite.Application.DTOs.Store;
using ByteBite.Application.DTOs.Common;
using TestDataGenerator = ByteBite.IntegrationTests.Helpers.TestDataGenerator;

namespace ByteBite.IntegrationTests.Modules;

/// <summary>
/// 店铺模块集成测试
/// </summary>
public class StoresIntegrationTests : IClassFixture<ByteBiteWebAppFactory>
{
    private readonly HttpClient _client;

    public StoresIntegrationTests(ByteBiteWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// 辅助方法：注册商家并返回商家ID
    /// </summary>
    private async Task<Guid> RegisterMerchantAndGetIdAsync()
    {
        var request = TestDataGenerator.CreateMerchantRegisterRequest();
        var response = await _client.PostAsJsonAsync("/api/merchants/register", request);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MerchantDto>>();
        return result!.Data!.Id;
    }

    /// <summary>
    /// 创建店铺 - 有效请求应返回成功且营业状态为open
    /// </summary>
    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var merchantId = await RegisterMerchantAndGetIdAsync();
        var request = TestDataGenerator.CreateStoreRequest(merchantId);

        // Act
        var response = await _client.PostAsJsonAsync("/api/stores", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<StoreDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.BusinessStatus.Should().Be("open");
    }

    /// <summary>
    /// 创建店铺后根据ID查询应返回正确店铺信息
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_AfterCreate_ReturnsStore()
    {
        // Arrange - 先创建店铺
        var merchantId = await RegisterMerchantAndGetIdAsync();
        var createRequest = TestDataGenerator.CreateStoreRequest(merchantId);
        var createResponse = await _client.PostAsJsonAsync("/api/stores", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<StoreDto>>();
        var storeId = createResult!.Data!.Id;

        // Act
        var response = await _client.GetAsync($"/api/stores/{storeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<StoreDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(storeId);
        result.Data.Name.Should().Be(createRequest.Name);
    }

    /// <summary>
    /// 根据商家ID查询店铺列表应包含已创建的店铺
    /// </summary>
    [Fact]
    public async Task GetByMerchantIdAsync_ReturnsStores()
    {
        // Arrange - 先创建店铺
        var merchantId = await RegisterMerchantAndGetIdAsync();
        var createRequest = TestDataGenerator.CreateStoreRequest(merchantId);
        await _client.PostAsJsonAsync("/api/stores", createRequest);

        // Act
        var response = await _client.GetAsync($"/api/stores/merchant/{merchantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<StoreDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// 切换营业状态 - 从营业中切换为休息中
    /// </summary>
    [Fact]
    public async Task ToggleBusinessStatusAsync_OpenToClosed()
    {
        // Arrange - 先创建店铺
        var merchantId = await RegisterMerchantAndGetIdAsync();
        var createRequest = TestDataGenerator.CreateStoreRequest(merchantId);
        var createResponse = await _client.PostAsJsonAsync("/api/stores", createRequest);
        var createResult = await createResponse.Content.ReadFromJsonAsync<ApiResponse<StoreDto>>();
        var storeId = createResult!.Data!.Id;
        var originalStatus = createResult.Data.BusinessStatus;

        // Act
        var response = await _client.PatchAsync($"/api/stores/{storeId}/toggle-status", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<StoreDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.BusinessStatus.Should().NotBe(originalStatus);
    }
}
