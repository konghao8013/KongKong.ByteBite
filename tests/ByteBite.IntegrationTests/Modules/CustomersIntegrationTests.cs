using ByteBite.Application.DTOs.Customer;
using ByteBite.Application.DTOs.Common;
using TestDataGenerator = ByteBite.IntegrationTests.Helpers.TestDataGenerator;

namespace ByteBite.IntegrationTests.Modules;

/// <summary>
/// 顾客模块集成测试 - 验证匿名顾客创建、注册、查询及数据合并预览
/// </summary>
public class CustomersIntegrationTests : IClassFixture<ByteBiteWebAppFactory>
{
    private readonly HttpClient _client;

    public CustomersIntegrationTests(ByteBiteWebAppFactory factory)
    {
        _client = factory.CreateClientWithNoAuth();
    }

    /// <summary>
    /// 测试：确保匿名顾客存在 - 新设备应返回未注册的顾客
    /// </summary>
    [Fact]
    public async Task EnsureAnonymousAsync_NewDevice_ReturnsCustomer()
    {
        var deviceId = $"test_device_{Guid.NewGuid():N}";

        var response = await _client.PostAsync($"/api/customers/anonymous?deviceId={deviceId}", null);

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.IsRegistered.Should().BeFalse();
    }

    /// <summary>
    /// 测试：顾客注册 - 有效请求应返回已注册的顾客
    /// </summary>
    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsCustomer()
    {
        var request = TestDataGenerator.CreateCustomerRegisterRequest();

        var response = await _client.PostAsJsonAsync("/api/customers/register", request);

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.IsRegistered.Should().BeTrue();
    }

    /// <summary>
    /// 测试：注册后根据ID查询顾客 - 应返回对应顾客信息
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_AfterRegister_ReturnsCustomer()
    {
        var request = TestDataGenerator.CreateCustomerRegisterRequest();
        var registerResponse = await _client.PostAsJsonAsync("/api/customers/register", request);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
        var customerId = registerResult!.Data!.Id;

        var response = await _client.GetAsync($"/api/customers/{customerId}");

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(customerId);
    }

    /// <summary>
    /// 测试：合并预览 - 根据设备ID查询应返回合并预览数据
    /// </summary>
    [Fact]
    public async Task MergePreviewAsync_ReturnsPreview()
    {
        var deviceId = $"test_device_{Guid.NewGuid():N}";

        var response = await _client.GetAsync($"/api/customers/merge-preview?deviceId={deviceId}");

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<DataMergeResultDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
    }
}
