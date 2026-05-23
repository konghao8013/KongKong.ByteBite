using ByteBite.Application.DTOs.Product;
using ByteBite.Application.Interfaces;
using ByteBite.Application.Services;

namespace ByteBite.UnitTests.Services;

/// <summary>
/// 商品服务单元测试
/// </summary>
public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _productRepoMock;
    private readonly Mock<ICategoryRepository> _categoryRepoMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _productRepoMock = new Mock<IProductRepository>();
        _categoryRepoMock = new Mock<ICategoryRepository>();
        _service = new ProductService(_productRepoMock.Object, _categoryRepoMock.Object);
    }

    /// <summary>创建示例商品实体</summary>
    private static ProductEntity CreateSampleProduct(
        string name = "测试商品",
        string status = "off",
        Guid? storeId = null,
        Guid? categoryId = null)
    {
        return new ProductEntity
        {
            Id = Guid.NewGuid(),
            StoreId = storeId ?? Guid.NewGuid(),
            CategoryId = categoryId ?? Guid.NewGuid(),
            CategoryName = "测试分类",
            Name = name,
            BasePrice = 10.0m,
            Status = status,
            SortOrder = 0,
            MinOrderQty = 1,
            MonthlySales = 0,
            TotalSales = 0,
            IsCombo = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            SpecGroups = []
        };
    }

    /// <summary>创建示例分类实体</summary>
    private static CategoryEntity CreateSampleCategory(string name = "测试分类")
    {
        return new CategoryEntity
        {
            Id = Guid.NewGuid(),
            StoreId = Guid.NewGuid(),
            Name = name,
            CategoryType = "normal",
            SortOrder = 0,
            IsVisible = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsProductDto()
    {
        var category = CreateSampleCategory();

        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        _productRepoMock
            .Setup(r => r.AddAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _productRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var storeId = Guid.NewGuid();
        var request = new CreateProductRequest
        {
            CategoryId = category.Id,
            Name = "新商品",
            BasePrice = 15.0m
        };

        var result = await _service.CreateAsync(storeId, request);

        // 验证AddAsync和SaveChangesAsync被调用
        _productRepoMock.Verify(r => r.AddAsync(It.IsAny<ProductEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _productRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // 新建商品默认状态为"off"（下架），名称匹配
        result.Status.Should().Be("off");
        result.Name.Should().Be("新商品");
    }

    [Fact]
    public async Task CreateAsync_NonExistingCategory_ThrowsKeyNotFoundException()
    {
        var categoryId = Guid.NewGuid();
        _categoryRepoMock
            .Setup(r => r.GetByIdAsync(categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CategoryEntity?)null);

        var storeId = Guid.NewGuid();
        var request = new CreateProductRequest
        {
            CategoryId = categoryId,
            Name = "新商品",
            BasePrice = 15.0m
        };

        var act = async () => await _service.CreateAsync(storeId, request);

        // 分类不存在应抛出KeyNotFoundException
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("分类不存在");
    }

    [Fact]
    public async Task UpdateAsync_ExistingProduct_UpdatesName()
    {
        var product = CreateSampleProduct(name: "旧商品名");

        _productRepoMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _productRepoMock
            .Setup(r => r.Update(It.IsAny<ProductEntity>()))
            .Callback<ProductEntity>(p => p.UpdatedAt = DateTime.UtcNow);

        _productRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new UpdateProductRequest { Name = "新商品名" };
        var result = await _service.UpdateAsync(product.Id, request);

        // 验证名称已更新
        result.Name.Should().Be("新商品名");
    }

    [Fact]
    public async Task UpdateAsync_NonExistingProduct_ThrowsKeyNotFoundException()
    {
        var productId = Guid.NewGuid();
        _productRepoMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        var request = new UpdateProductRequest { Name = "新商品名" };
        var act = async () => await _service.UpdateAsync(productId, request);

        // 商品不存在应抛出KeyNotFoundException
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("商品不存在");
    }

    [Fact]
    public async Task DeleteAsync_ExistingProduct_SetsDeletedAt()
    {
        var product = CreateSampleProduct();

        _productRepoMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _productRepoMock
            .Setup(r => r.Update(It.IsAny<ProductEntity>()))
            .Callback<ProductEntity>(p => p.UpdatedAt = DateTime.UtcNow);

        _productRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.DeleteAsync(product.Id);

        // 验证DeletedAt已被设置
        product.DeletedAt.Should().NotBeNull();

        // 验证Update被调用（软删除通过Update实现）
        _productRepoMock.Verify(r => r.Update(It.IsAny<ProductEntity>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingProduct_ThrowsKeyNotFoundException()
    {
        var productId = Guid.NewGuid();
        _productRepoMock
            .Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        var act = async () => await _service.DeleteAsync(productId);

        // 商品不存在应抛出KeyNotFoundException
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("商品不存在");
    }

    [Fact]
    public async Task BatchUpdateStatusAsync_UpdatesMatchingProducts()
    {
        var storeId = Guid.NewGuid();
        var product1 = CreateSampleProduct(storeId: storeId);
        var product2 = CreateSampleProduct(storeId: storeId);
        var product3 = CreateSampleProduct(storeId: storeId);

        // 模拟店铺下有3个商品
        _productRepoMock
            .Setup(r => r.GetByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductEntity> { product1, product2, product3 });

        _productRepoMock
            .Setup(r => r.Update(It.IsAny<ProductEntity>()))
            .Callback<ProductEntity>(p => p.UpdatedAt = DateTime.UtcNow);

        _productRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // 只更新前2个商品的状态为"on"
        var targetIds = new List<Guid> { product1.Id, product2.Id };
        var result = await _service.BatchUpdateStatusAsync(storeId, targetIds, "on");

        // 验证只有2个商品被更新
        result.Should().HaveCount(2);
        result.All(p => p.Status == "on").Should().BeTrue();

        // 验证Update被调用了2次
        _productRepoMock.Verify(r => r.Update(It.IsAny<ProductEntity>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsProductDto()
    {
        var product = CreateSampleProduct();
        _productRepoMock
            .Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var result = await _service.GetByIdAsync(product.Id);

        // 存在的ID应返回ProductDto
        result.Should().NotBeNull();
        result!.Name.Should().Be(product.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _productRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((ProductEntity?)null);

        var result = await _service.GetByIdAsync(id);

        // 不存在的ID应返回null
        result.Should().BeNull();
    }
}