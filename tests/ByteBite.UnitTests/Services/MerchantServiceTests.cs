using ByteBite.Application.DTOs.Merchant;
using ByteBite.Application.Interfaces;
using ByteBite.Application.Services;
using ByteBite.Shared.Helpers;

namespace ByteBite.UnitTests.Services;

/// <summary>
/// 商家服务单元测试
/// </summary>
public class MerchantServiceTests
{
    private readonly Mock<IMerchantRepository> _merchantRepoMock;
    private readonly Mock<IStoreRepository> _storeRepoMock;
    private readonly MerchantService _service;

    public MerchantServiceTests()
    {
        _merchantRepoMock = new Mock<IMerchantRepository>();
        _storeRepoMock = new Mock<IStoreRepository>();
        _service = new MerchantService(_merchantRepoMock.Object, _storeRepoMock.Object);
    }

    /// <summary>创建示例商家实体</summary>
    private static MerchantEntity CreateSampleMerchant(
        string phone = "13800138000",
        string? passwordHash = null,
        string status = "active")
    {
        return new MerchantEntity
        {
            Id = Guid.NewGuid(),
            Phone = phone,
            PasswordHash = passwordHash ?? PasswordHasher.HashPassword("test123"),
            Status = status,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>创建示例注册请求</summary>
    private static RegisterMerchantRequest CreateSampleRegisterRequest(
        string phone = "13800138000",
        string password = "test123",
        string storeName = "测试店铺")
    {
        return new RegisterMerchantRequest
        {
            Phone = phone,
            Password = password,
            StoreName = storeName
        };
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsMerchantDto()
    {
        // 手机号不存在，可以注册
        _merchantRepoMock
            .Setup(r => r.ExistsByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _merchantRepoMock
            .Setup(r => r.AddAsync(It.IsAny<MerchantEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _storeRepoMock
            .Setup(r => r.AddAsync(It.IsAny<StoreEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _merchantRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = CreateSampleRegisterRequest();
        var result = await _service.RegisterAsync(request);

        // 验证两个仓储的AddAsync都被调用
        _merchantRepoMock.Verify(r => r.AddAsync(It.IsAny<MerchantEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _storeRepoMock.Verify(r => r.AddAsync(It.IsAny<StoreEntity>(), It.IsAny<CancellationToken>()), Times.Once);

        // 验证SaveChangesAsync被调用
        _merchantRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // 验证返回结果的手机号和状态
        result.Phone.Should().Be(request.Phone);
        result.Status.Should().Be("active");
    }

    [Fact]
    public async Task RegisterAsync_DuplicatePhone_ThrowsInvalidOperationException()
    {
        // 手机号已存在
        _merchantRepoMock
            .Setup(r => r.ExistsByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = CreateSampleRegisterRequest();
        var act = async () => await _service.RegisterAsync(request);

        // 应抛出InvalidOperationException，提示"该手机号已注册"
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("该手机号已注册");
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsMerchantDto()
    {
        // 预先计算密码哈希，确保密码验证能通过
        var knownHash = PasswordHasher.HashPassword("test123");
        var merchant = CreateSampleMerchant(passwordHash: knownHash);

        _merchantRepoMock
            .Setup(r => r.GetByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchant);

        _merchantRepoMock
            .Setup(r => r.Update(It.IsAny<MerchantEntity>()))
            .Callback<MerchantEntity>(m => m.UpdatedAt = DateTime.UtcNow);

        _merchantRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new LoginMerchantRequest { Phone = "13800138000", Password = "test123" };
        var result = await _service.LoginAsync(request);

        // 验证Update被调用（更新登录时间）
        _merchantRepoMock.Verify(r => r.Update(It.IsAny<MerchantEntity>()), Times.Once);

        // 验证返回结果不为空
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        // 商家密码哈希与请求密码不匹配
        var merchant = CreateSampleMerchant(passwordHash: PasswordHasher.HashPassword("different_password"));

        _merchantRepoMock
            .Setup(r => r.GetByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchant);

        var request = new LoginMerchantRequest { Phone = "13800138000", Password = "test123" };
        var result = await _service.LoginAsync(request);

        // 密码不匹配，应返回null
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_PhoneNotFound_ReturnsNull()
    {
        // 手机号不存在
        _merchantRepoMock
            .Setup(r => r.GetByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync((MerchantEntity?)null);

        var request = new LoginMerchantRequest { Phone = "13800138000", Password = "test123" };
        var result = await _service.LoginAsync(request);

        // 手机号不存在，应返回null
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMerchantDto()
    {
        var merchant = CreateSampleMerchant();
        _merchantRepoMock
            .Setup(r => r.GetByIdAsync(merchant.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchant);

        var result = await _service.GetByIdAsync(merchant.Id);

        // 存在的ID应返回MerchantDto
        result.Should().NotBeNull();
        result!.Phone.Should().Be(merchant.Phone);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _merchantRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MerchantEntity?)null);

        var result = await _service.GetByIdAsync(id);

        // 不存在的ID应返回null
        result.Should().BeNull();
    }
}