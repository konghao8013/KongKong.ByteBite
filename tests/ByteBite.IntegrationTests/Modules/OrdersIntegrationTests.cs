using System.Net;
using System.Net.Http.Json;
using ByteBite.Application.DTOs.Category;
using ByteBite.Application.DTOs.Common;
using ByteBite.Application.DTOs.Customer;
using ByteBite.Application.DTOs.Merchant;
using ByteBite.Application.DTOs.Order;
using ByteBite.Application.DTOs.Product;
using ByteBite.Application.DTOs.Store;
using ByteBite.IntegrationTests.Helpers;

namespace ByteBite.IntegrationTests.Modules;

/// <summary>
/// 订单模块集成测试
/// </summary>
public class OrdersIntegrationTests : IClassFixture<ByteBiteWebAppFactory>
{
    private readonly HttpClient _client;

    public OrdersIntegrationTests(ByteBiteWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// 辅助方法：创建完整的测试数据（商家→店铺→分类→商品→匿名顾客），返回所需ID
    /// </summary>
    private async Task<(Guid StoreId, Guid ProductId, Guid CustomerId)> CreateFullTestDataAsync()
    {
        // 注册商家
        var registerRequest = TestDataGenerator.CreateMerchantRegisterRequest();
        var registerResponse = await _client.PostAsJsonAsync("/api/merchants/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<MerchantDto>>();
        var merchantId = registerResult!.Data!.Id;

        // 创建店铺
        var storeRequest = TestDataGenerator.CreateStoreRequest(merchantId);
        var storeResponse = await _client.PostAsJsonAsync("/api/stores", storeRequest);
        storeResponse.EnsureSuccessStatusCode();
        var storeResult = await storeResponse.Content.ReadFromJsonAsync<ApiResponse<StoreDto>>();
        var storeId = storeResult!.Data!.Id;

        // 创建分类
        var categoryRequest = TestDataGenerator.CreateCategoryRequest();
        var categoryResponse = await _client.PostAsJsonAsync($"/api/stores/{storeId}/categories", categoryRequest);
        categoryResponse.EnsureSuccessStatusCode();
        var categoryResult = await categoryResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        var categoryId = categoryResult!.Data!.Id;

        // 创建商品
        var productRequest = TestDataGenerator.CreateProductRequest(categoryId);
        var productResponse = await _client.PostAsJsonAsync($"/api/stores/{storeId}/products", productRequest);
        productResponse.EnsureSuccessStatusCode();
        var productResult = await productResponse.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        var productId = productResult!.Data!.Id;

        // 创建匿名顾客
        var deviceId = $"device_test_{Guid.NewGuid():N}";
        var anonymousResponse = await _client.PostAsync($"/api/customers/anonymous?deviceId={deviceId}", null);
        anonymousResponse.EnsureSuccessStatusCode();
        var anonymousResult = await anonymousResponse.Content.ReadFromJsonAsync<ApiResponse<CustomerDto>>();
        var customerId = anonymousResult!.Data!.Id;

        return (storeId, productId, customerId);
    }

    /// <summary>
    /// 辅助方法：创建订单并返回订单DTO
    /// </summary>
    private async Task<OrderDto> CreateOrderAsync()
    {
        var (storeId, productId, _) = await CreateFullTestDataAsync();
        var orderItemRequest = TestDataGenerator.CreateOrderItemRequest(productId);
        var orderRequest = TestDataGenerator.CreateOrderRequest(storeId, [orderItemRequest]);

        var response = await _client.PostAsJsonAsync("/api/orders", orderRequest);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        return result!.Data!;
    }

    /// <summary>
    /// 创建订单 - 有效请求应返回带取货码的订单且状态为pending
    /// </summary>
    [Fact]
    public async Task CreateOrderAsync_ValidRequest_ReturnsOrderWithPickupCode()
    {
        // Arrange
        var (storeId, productId, _) = await CreateFullTestDataAsync();
        var orderItemRequest = TestDataGenerator.CreateOrderItemRequest(productId);
        var orderRequest = TestDataGenerator.CreateOrderRequest(storeId, [orderItemRequest]);

        // Act
        var response = await _client.PostAsJsonAsync("/api/orders", orderRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.PickupCode.Should().NotBeNullOrEmpty();
        result.Data.Status.Should().Be("pending");
    }

    /// <summary>
    /// 创建订单后查询店铺订单列表应包含该订单
    /// </summary>
    [Fact]
    public async Task GetStoreOrdersAsync_ReturnsOrder()
    {
        // Arrange - 先创建订单
        var (storeId, productId, _) = await CreateFullTestDataAsync();
        var orderItemRequest = TestDataGenerator.CreateOrderItemRequest(productId);
        var orderRequest = TestDataGenerator.CreateOrderRequest(storeId, [orderItemRequest]);
        await _client.PostAsJsonAsync("/api/orders", orderRequest);

        // Act
        var response = await _client.GetAsync($"/api/stores/{storeId}/orders");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<PagedResult<OrderDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Items.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// 接单操作应将订单状态变为accepted
    /// </summary>
    [Fact]
    public async Task AcceptOrderAsync_ChangesStatusToAccepted()
    {
        // Arrange - 先创建订单
        var order = await CreateOrderAsync();

        // Act
        var response = await _client.PatchAsync($"/api/orders/{order.Id}/accept", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be("accepted");
    }

    /// <summary>
    /// 完整订单流程：pending → accepted → preparing → ready → completed
    /// </summary>
    [Fact]
    public async Task FullOrderFlow_PendingToCompleted()
    {
        // Arrange - 创建订单
        var order = await CreateOrderAsync();
        order.Status.Should().Be("pending");

        // Act & Assert - 接单
        var acceptResponse = await _client.PatchAsync($"/api/orders/{order.Id}/accept", null);
        acceptResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var acceptResult = await acceptResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        acceptResult!.Data!.Status.Should().Be("accepted");

        // Act & Assert - 制作中
        var prepareResponse = await _client.PatchAsync($"/api/orders/{order.Id}/prepare", null);
        prepareResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var prepareResult = await prepareResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        prepareResult!.Data!.Status.Should().Be("preparing");

        // Act & Assert - 备餐完毕
        var readyResponse = await _client.PatchAsync($"/api/orders/{order.Id}/ready", null);
        readyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var readyResult = await readyResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        readyResult!.Data!.Status.Should().Be("ready");

        // Act & Assert - 完成订单
        var completeResponse = await _client.PatchAsync($"/api/orders/{order.Id}/complete", null);
        completeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var completeResult = await completeResponse.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        completeResult!.Data!.Status.Should().Be("completed");
    }

    /// <summary>
    /// 拒单操作应将订单状态变为cancelled
    /// </summary>
    [Fact]
    public async Task RejectOrderAsync_ChangesStatusToCancelled()
    {
        // Arrange - 先创建订单
        var order = await CreateOrderAsync();
        var rejectRequest = new { Reason = "测试拒单原因" };

        // Act
        var response = await _client.PatchAsJsonAsync($"/api/orders/{order.Id}/reject", rejectRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<OrderDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Status.Should().Be("cancelled");
    }
}
