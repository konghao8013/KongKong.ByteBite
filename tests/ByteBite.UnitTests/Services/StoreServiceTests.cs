using ByteBite.Application.DTOs.Store;
using ByteBite.Application.Interfaces;
using ByteBite.Application.Services;

namespace ByteBite.UnitTests.Services;

/// <summary>
/// 店铺服务单元测试
/// </summary>
public class StoreServiceTests
{
    private readonly Mock<IStoreRepository> _storeRepoMock;
    private readonly StoreService _service;

    public StoreServiceTests()
    {
        _storeRepoMock = new Mock<IStoreRepository>();
        _service = new StoreService(_storeRepoMock.Object);
    }

    /// <summary>创建示例店铺实体</summary>
    private static StoreEntity CreateSampleStore(
        string name = "测试店铺",
        string businessStatus = "open",
        Guid? merchantId = null)
    {
        return new StoreEntity
        {
            Id = Guid.NewGuid(),
            MerchantId = merchantId ?? Guid.NewGuid(),
            Name = name,
            BusinessStatus = businessStatus,
            DiningMode = "dine_in,takeaway",
            MonthlySales = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsStoreDto()
    {
        _storeRepoMock
            .Setup(r => r.AddAsync(It.IsAny<StoreEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _storeRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var merchantId = Guid.NewGuid();
        var request = new CreateStoreRequest { Name = "新店铺" };
        var result = await _service.CreateAsync(merchantId, request);

        // 验证AddAsync和SaveChangesAsync被调用
        _storeRepoMock.Verify(r => r.AddAsync(It.IsAny<StoreEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _storeRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // 验证返回结果的名称和营业状态
        result.Name.Should().Be("新店铺");
        result.BusinessStatus.Should().Be("open");
    }

    [Fact]
    public async Task UpdateAsync_ExistingStore_UpdatesFields()
    {
        var store = CreateSampleStore(name: "旧名称");

        _storeRepoMock
            .Setup(r => r.GetByIdAsync(store.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        _storeRepoMock
            .Setup(r => r.Update(It.IsAny<StoreEntity>()))
            .Callback<StoreEntity>(s => s.UpdatedAt = DateTime.UtcNow);

        _storeRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new UpdateStoreRequest { Name = "新名称" };
        var result = await _service.UpdateAsync(store.Id, request);

        // 验证Update和SaveChangesAsync被调用
        _storeRepoMock.Verify(r => r.Update(It.IsAny<StoreEntity>()), Times.Once);
        _storeRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // 验证名称已更新
        result.Name.Should().Be("新名称");
    }

    [Fact]
    public async Task UpdateAsync_NonExistingStore_ThrowsKeyNotFoundException()
    {
        var storeId = Guid.NewGuid();
        _storeRepoMock
            .Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreEntity?)null);

        var request = new UpdateStoreRequest { Name = "新名称" };
        var act = async () => await _service.UpdateAsync(storeId, request);

        // 不存在的店铺应抛出KeyNotFoundException
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("店铺不存在");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsStoreDto()
    {
        var store = CreateSampleStore();
        _storeRepoMock
            .Setup(r => r.GetByIdAsync(store.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        var result = await _service.GetByIdAsync(store.Id);

        // 存在的ID应返回StoreDto
        result.Should().NotBeNull();
        result!.Name.Should().Be(store.Name);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _storeRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreEntity?)null);

        var result = await _service.GetByIdAsync(id);

        // 不存在的ID应返回null
        result.Should().BeNull();
    }

    [Fact]
    public async Task ToggleBusinessStatusAsync_OpenToClosed()
    {
        // 营业中的店铺切换为休息中
        var store = CreateSampleStore(businessStatus: "open");

        _storeRepoMock
            .Setup(r => r.GetByIdAsync(store.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        _storeRepoMock
            .Setup(r => r.Update(It.IsAny<StoreEntity>()))
            .Callback<StoreEntity>(s => s.UpdatedAt = DateTime.UtcNow);

        _storeRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.ToggleBusinessStatusAsync(store.Id);

        // 从"open"切换为"closed"
        result.BusinessStatus.Should().Be("closed");
    }

    [Fact]
    public async Task ToggleBusinessStatusAsync_ClosedToOpen()
    {
        // 休息中的店铺切换为营业中
        var store = CreateSampleStore(businessStatus: "closed");

        _storeRepoMock
            .Setup(r => r.GetByIdAsync(store.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        _storeRepoMock
            .Setup(r => r.Update(It.IsAny<StoreEntity>()))
            .Callback<StoreEntity>(s => s.UpdatedAt = DateTime.UtcNow);

        _storeRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.ToggleBusinessStatusAsync(store.Id);

        // 从"closed"切换为"open"
        result.BusinessStatus.Should().Be("open");
    }

    [Fact]
    public async Task ToggleBusinessStatusAsync_NonExisting_ThrowsKeyNotFoundException()
    {
        var storeId = Guid.NewGuid();
        _storeRepoMock
            .Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StoreEntity?)null);

        var act = async () => await _service.ToggleBusinessStatusAsync(storeId);

        // 不存在的店铺应抛出KeyNotFoundException
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("店铺不存在");
    }
}