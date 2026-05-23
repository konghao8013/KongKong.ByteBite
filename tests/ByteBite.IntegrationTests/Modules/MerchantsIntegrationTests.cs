using System.Net;
using System.Net.Http.Json;
using ByteBite.Application.DTOs.Merchant;
using ByteBite.Application.DTOs.Common;
using TestDataGenerator = ByteBite.IntegrationTests.Helpers.TestDataGenerator;

namespace ByteBite.IntegrationTests.Modules;

/// <summary>
/// 商家模块集成测试
/// </summary>
public class MerchantsIntegrationTests : IClassFixture<ByteBiteWebAppFactory>
{
    private readonly HttpClient _client;

    public MerchantsIntegrationTests(ByteBiteWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// 注册商家 - 有效请求应返回成功
    /// </summary>
    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var request = TestDataGenerator.CreateMerchantRegisterRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/merchants/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MerchantDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Phone.Should().Be(request.Phone);
        result.Data.Status.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// 重复手机号注册应返回失败
    /// </summary>
    [Fact]
    public async Task RegisterAsync_DuplicatePhone_ReturnsBadRequest()
    {
        // Arrange - 第一次注册
        var phone = TestDataGenerator.GeneratePhone();
        var firstRequest = TestDataGenerator.CreateMerchantRegisterRequest(phone);
        await _client.PostAsJsonAsync("/api/merchants/register", firstRequest);

        // Arrange - 第二次使用相同手机号注册
        var secondRequest = TestDataGenerator.CreateMerchantRegisterRequest(phone);

        // Act
        var response = await _client.PostAsJsonAsync("/api/merchants/register", secondRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    /// <summary>
    /// 商家登录 - 正确凭据应返回成功及Token
    /// </summary>
    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsToken()
    {
        // Arrange - 先注册商家
        var registerRequest = TestDataGenerator.CreateMerchantRegisterRequest();
        await _client.PostAsJsonAsync("/api/merchants/register", registerRequest);

        // Arrange - 构造登录请求
        var loginRequest = TestDataGenerator.CreateMerchantLoginRequest(registerRequest.Phone);

        // Act
        var response = await _client.PostAsJsonAsync("/api/merchants/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MerchantDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Phone.Should().Be(registerRequest.Phone);
        result.Data.Token.Should().NotBeNullOrEmpty();
    }

    /// <summary>
    /// 商家登录 - 错误密码应返回401
    /// </summary>
    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsUnauthorized()
    {
        // Arrange - 先注册商家
        var registerRequest = TestDataGenerator.CreateMerchantRegisterRequest();
        await _client.PostAsJsonAsync("/api/merchants/register", registerRequest);

        // Arrange - 使用错误密码登录
        var loginRequest = new LoginMerchantRequest
        {
            Phone = registerRequest.Phone,
            Password = "WrongPassword123!"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/merchants/login", loginRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    /// <summary>
    /// 根据ID获取商家 - 存在的商家应返回正确信息
    /// </summary>
    [Fact]
    public async Task GetByIdAsync_ExistingMerchant_ReturnsMerchant()
    {
        // Arrange - 先注册商家获取ID
        var registerRequest = TestDataGenerator.CreateMerchantRegisterRequest();
        var registerResponse = await _client.PostAsJsonAsync("/api/merchants/register", registerRequest);
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<MerchantDto>>();
        var merchantId = registerResult!.Data!.Id;

        // Act
        var response = await _client.GetAsync($"/api/merchants/{merchantId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<MerchantDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Id.Should().Be(merchantId);
        result.Data.Phone.Should().Be(registerRequest.Phone);
    }
}
