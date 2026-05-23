using ByteBite.Application.DTOs.Category;
using ByteBite.Application.Interfaces;
using ByteBite.Application.Services;

namespace ByteBite.UnitTests.Services;

/// <summary>
/// 分类服务单元测试
/// </summary>
public class CategoryServiceTests
{
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly CategoryService _service;

    public CategoryServiceTests()
    {
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _service = new CategoryService(_categoryRepoMock.Object);
    }

    /// <summary>创建示例分类实体</summary>
    private static CategoryEntity CreateSampleCategory(
        string name = "测试分类",
        int sortOrder = 0,
        Guid? storeId = null)
    {
        return new CategoryEntity
        {
            Id = Guid.NewGuid(),
            StoreId = storeId ?? Guid.NewGuid(),
            Name = name,
            CategoryType = "normal",
            SortOrder = sortOrder,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCategoryDto()
    {
        _categoryRepoMock
            .Setup(r => r.AddAsync(It.IsAny<CategoryEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _categoryRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var storeId = Guid.NewGuid();
        var request = new CreateCategoryRequest { Name = "热销推荐", CategoryType = "hot", SortOrder = 1 };
        var result = await _service.CreateAsync(storeId, request);

        // 验证AddAsync和SaveChangesAsync被调用
        _categoryRepoMock.Verify(r => r.AddAsync(It.IsAny<CategoryEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // 新建分类商品数量应为0，名称匹配
        result.ProductCount.Should().Be(0);
        result.Name.Should().Be("热销推荐");
    }

    [Fact]
    public async Task UpdateAsync_ExistingCategory_UpdatesName()
    {
        var category = CreateSampleCategory(name: "旧分类名");

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepoMock
            .Setup(r => r.GetProductCountByCategoryIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _categoryRepoMock
            .Setup(r => r.Update(It.IsAny<CategoryEntity>()))
            .Callback<CategoryEntity>(c => c.UpdatedAt = DateTime.UtcNow);

        _categoryRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new UpdateCategoryRequest { Name = "新分类名" };
        var result = await _service.UpdateAsync(category.Id, request);

        // 验证名称已更新
        result.Name.Should().Be("新分类名");
    }

    [Fact]
    public async Task UpdateAsync_NonExistingCategory_ThrowsKeyNotFoundException()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryEntity?)null);

        var request = new UpdateCategoryRequest { Name = "新分类名" };
        var act = async () => await _service.UpdateAsync(categoryId, request);

        // 不存在的分类应抛出KeyNotFoundException
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("分类不存在");
    }

    [Fact]
    public async Task DeleteAsync_ExistingCategory_SetsDeletedAt()
    {
        var category = CreateSampleCategory();

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepoMock
            .Setup(r => r.Update(It.IsAny<CategoryEntity>()))
            .Callback<CategoryEntity>(c => c.UpdatedAt = DateTime.UtcNow);

        _categoryRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.DeleteAsync(category.Id);

        // 验证DeletedAt已被设置
        category.DeletedAt.Should().NotBeNull();

        // 验证Update被调用（软删除通过Update实现）
        _categoryRepoMock.Verify(r => r.Update(It.IsAny<CategoryEntity>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingCategory_ThrowsKeyNotFoundException()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryEntity?)null);

        var act = async () => await _service.DeleteAsync(categoryId);

        // 不存在的分类应抛出KeyNotFoundException
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("分类不存在");
    }

    [Fact]
    public async Task UpdateSortOrderAsync_ExistingCategory_UpdatesSortOrder()
    {
        var category = CreateSampleCategory(sortOrder: 0);

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _categoryRepoMock
            .Setup(r => r.GetProductCountByCategoryIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(2);

        _categoryRepoMock
            .Setup(r => r.Update(It.IsAny<CategoryEntity>()))
            .Callback<CategoryEntity>(c => c.UpdatedAt = DateTime.UtcNow);

        _categoryRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateSortOrderAsync(category.Id, 5);

        // 验证排序序号已更新
        result.SortOrder.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsWithProductCount()
    {
        var category = CreateSampleCategory();

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        // 该分类下有5个商品
        _categoryRepoMock
            .Setup(r => r.GetProductCountByCategoryIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);

        var result = await _service.GetByIdAsync(category.Id);

        // 验证返回结果包含正确的商品数量
        result.Should().NotBeNull();
        result!.ProductCount.Should().Be(5);
    }
}