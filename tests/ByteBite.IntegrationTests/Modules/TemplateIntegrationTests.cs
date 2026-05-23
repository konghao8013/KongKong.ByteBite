using ByteBite.Application.DTOs.Template;
using ByteBite.Application.DTOs.Common;
using TestDataGenerator = ByteBite.IntegrationTests.Helpers.TestDataGenerator;

namespace ByteBite.IntegrationTests.Modules;

/// <summary>
/// 模板与行业分类模块集成测试 - 验证行业分类树、模板创建及模板列表查询
/// </summary>
public class TemplateIntegrationTests : IClassFixture<ByteBiteWebAppFactory>
{
    private readonly HttpClient _client;

    public TemplateIntegrationTests(ByteBiteWebAppFactory factory)
    {
        _client = factory.CreateClientWithNoAuth();
    }

    /// <summary>
    /// 测试：获取行业分类树 - 应返回树形结构数据
    /// </summary>
    [Fact]
    public async Task GetIndustryCategoryTreeAsync_ReturnsTree()
    {
        var response = await _client.GetAsync("/api/industry-categories/tree");

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<IndustryCategoryDto>>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
    }

    /// <summary>
    /// 测试：从零创建模板 - 应返回创建的模板，验证名称
    /// </summary>
    [Fact]
    public async Task CreateTemplateFromScratchAsync_ReturnsCreated()
    {
        var treeResponse = await _client.GetAsync("/api/industry-categories/tree");
        var treeResult = await treeResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<IndustryCategoryDto>>>();
        var industryCategoryId = treeResult!.Data!.FirstOrDefault()?.Id;

        var request = new
        {
            Name = $"测试模板_{Guid.NewGuid():N}",
            IndustryCategoryId = industryCategoryId,
            Description = "集成测试创建的模板"
        };

        var response = await _client.PostAsJsonAsync("/api/templates/from-scratch", request);

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<TemplateDto>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Name.Should().Be(request.Name);
    }

    /// <summary>
    /// 测试：获取模板列表 - 创建模板后查询应至少包含1条
    /// </summary>
    [Fact]
    public async Task GetTemplatesAsync_ReturnsList()
    {
        var treeResponse = await _client.GetAsync("/api/industry-categories/tree");
        var treeResult = await treeResponse.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<IndustryCategoryDto>>>();
        var industryCategoryId = treeResult!.Data!.FirstOrDefault()?.Id;

        var createRequest = new
        {
            Name = $"列表测试模板_{Guid.NewGuid():N}",
            IndustryCategoryId = industryCategoryId,
            Description = "用于列表查询测试"
        };
        await _client.PostAsJsonAsync("/api/templates/from-scratch", createRequest);

        var response = await _client.GetAsync("/api/templates");

        response.Should().BeSuccessful();
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<IReadOnlyList<TemplateDto>>>();
        result.Should().NotBeNull();
        result!.Code.Should().Be(200);
        result.Data.Should().NotBeNull();
        result.Data!.Should().HaveCountGreaterThanOrEqualTo(1);
    }
}
