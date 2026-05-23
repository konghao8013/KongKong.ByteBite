using System.Net;
using System.Net.Http.Json;
using ByteBite.Application.DTOs.Category;
using ByteBite.Application.DTOs.Customer;
using ByteBite.Application.DTOs.Merchant;
using ByteBite.Application.DTOs.Product;
using ByteBite.Application.DTOs.Store;
using ByteBite.Application.DTOs.Common;
using TestDataGenerator = ByteBite.IntegrationTests.Helpers.TestDataGenerator;

namespace ByteBite.IntegrationTests.Modules;

/// <summary>
/// 分类与商品模块集成测试
/// </summary>
public class CategoriesAndProductsIntegrationTests : IClassFixture<ByteBiteWebAppFactory>
{
    private readonly HttpClient _client;

    public CategoriesAndProductsIntegrationTests(ByteBiteWebAppFactory factory)
    {
        _client = factory.CreateClient();
    }

    /// <summary>
    /// 辅助方法：注册商家并创建店铺，返回店铺ID
    /// </summary>
    private async Task<Guid> CreateStoreAndGetIdAsync()
    {
        var registerRequest = TestDataGenerator.CreateMerchantRegisterRequest();
        var registerResponse = await _client.PostAsJsonAsync("/api/merchants/register", registerRequest);
        registerResponse.EnsureSuccessStatusCode();
        var registerResult = await registerResponse.Content.ReadFromJsonAsync<ApiResponse<MerchantDto>>();
        var merchantId = registerResult!.Data!.Id;

        var storeRequest = TestDataGenerator.CreateStoreRequest(merchantId);
        var storeResponse = await _client.PostAsJsonAsync("/api/stores", storeRequest);
        storeResponse.EnsureSuccessStatusCode();
        var storeResult = await storeResponse.Content.ReadFromJsonAsync<ApiResponse<StoreDto>>();
        return storeResult!.Data!.Id;
    }

    /// <summary>
    /// 辅助方法：创建分类并返回分类ID
    /// </summary>
    private async Task<(Guid StoreId, Guid CategoryId)> CreateCategoryAndGetIdAsync()
    {
        var storeId = await CreateStoreAndGetIdAsync();
        var categoryRequest = TestDataGenerator.CreateCategoryRequest();
        var categoryResponse = await _client.PostAsJsonAsync($"/api/stores/{storeId}/categories", categoryRequest);
        categoryResponse.EnsureSuccessStatusCode();
        var categoryResult = await categoryResponse.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        return (storeId, categoryResult!.Data!.Id);
    }

    /// <summary>
    /// 创建分类 - 有效请求应返回成功且名称匹配
    /// </summary>
    [Fact]
    public async Task CreateCategoryAsync_ValidRequest_ReturnsCreated()
    {
        // Arrange
        var storeId = await CreateStoreAndGetIdAsync();
        var request = TestDataGenerator.CreateCategoryRequest();

        // Act
        var response = await _client.PostAsJsonAsync($"/api/stores/{storeId}/categories", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<CategoryDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(request.Name);
    }

    /// <summary>
    /// 创建分类后查询店铺分类列表应包含该分类
    /// </summary>
    [Fact]
    public async Task GetCategoriesByStoreAsync_ReturnsList()
    {
        // Arrange - 先创建分类
        var storeId = await CreateStoreAndGetIdAsync();
        var categoryRequest = TestDataGenerator.CreateCategoryRequest();
        await _client.PostAsJsonAsync($"/api/stores/{storeId}/categories", categoryRequest);

        // Act
        var response = await _client.GetAsync($"/api/stores/{storeId}/categories");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// 创建商品 - 有效请求应返回成功且名称和价格匹配
    /// </summary>
    [Fact]
    public async Task CreateProductAsync_ValidRequest_ReturnsCreated()
    {
        // Arrange - 先创建分类
        var (storeId, categoryId) = await CreateCategoryAndGetIdAsync();
        var request = TestDataGenerator.CreateProductRequest(categoryId);

        // Act
        var response = await _client.PostAsJsonAsync($"/api/stores/{storeId}/products", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ProductDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(request.Name);
        result.Data.BasePrice.Should().Be(request.BasePrice);
    }

    /// <summary>
    /// 创建商品后查询店铺商品列表应包含该商品
    /// </summary>
    [Fact]
    public async Task GetProductsByStoreAsync_ReturnsList()
    {
        // Arrange - 先创建分类和商品
        var (storeId, categoryId) = await CreateCategoryAndGetIdAsync();
        var productRequest = TestDataGenerator.CreateProductRequest(categoryId);
        await _client.PostAsJsonAsync($"/api/stores/{storeId}/products", productRequest);

        // Act
        var response = await _client.GetAsync($"/api/stores/{storeId}/products");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<List<ProductDto>>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// 顾客端菜单查询应返回分类和商品
    /// </summary>
    [Fact]
    public async Task GetStoreMenuAsync_ReturnsMenu()
    {
        // Arrange - 创建分类和商品
        var (storeId, categoryId) = await CreateCategoryAndGetIdAsync();
        var productRequest = TestDataGenerator.CreateProductRequest(categoryId);
        await _client.PostAsJsonAsync($"/api/stores/{storeId}/products", productRequest);

        // Act
        var response = await _client.GetAsync($"/api/customer/stores/{storeId}/menu");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<StoreMenuDto>>();
        result.Should().NotBeNull();
        result!.Data.Should().NotBeNull();
        result.Data!.Categories.Should().NotBeNull();
        result.Data.Categories.Should().HaveCountGreaterThanOrEqualTo(1);
        result.Data.Categories[0].Items.Should().HaveCountGreaterThanOrEqualTo(1);
    }

    /// <summary>
    /// 删除分类后查询列表不应包含已删除分类
    /// </summary>
    [Fact]
    public async Task DeleteCategoryAsync_SoftDeletes()
    {
        // Arrange - 先创建分类
        var (storeId, categoryId) = await CreateCategoryAndGetIdAsync();

        // Act - 删除分类
        var deleteResponse = await _client.DeleteAsync($"/api/categories/{categoryId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - 查询分类列表不应包含已删除分类
        var listResponse = await _client.GetAsync($"/api/stores/{storeId}/categories");
        listResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResult = await listResponse.Content.ReadFromJsonAsync<ApiResponse<List<CategoryDto>>>();
        listResult.Should().NotBeNull();
        listResult!.Data.Should().NotBeNull();
        listResult.Data.Should().NotContain(c => c.Id == categoryId);
    }
}
