using ByteBite.Application.DTOs.Customer;
using ByteBite.Application.Interfaces;
using ByteBite.Application.Services;

namespace ByteBite.UnitTests.Services;

/// <summary>
/// 顾客服务单元测试
/// </summary>
public class CustomerServiceTests
{
    private readonly Mock<ICustomerRepository> _customerRepoMock;
    private readonly CustomerService _service;

    public CustomerServiceTests()
    {
        _customerRepoMock = new Mock<ICustomerRepository>();
        _service = new CustomerService(_customerRepoMock.Object);
    }

    /// <summary>创建示例顾客实体</summary>
    private static CustomerEntity CreateSampleCustomer(
        string? phone = "13800138000",
        string? nickname = "测试用户",
        bool isRegistered = true,
        string? deviceId = null)
    {
        return new CustomerEntity
        {
            Id = Guid.NewGuid(),
            Phone = phone,
            Nickname = nickname,
            DeviceId = deviceId,
            IsRegistered = isRegistered,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    [Fact]
    public async Task RegisterAsync_ValidRequest_ReturnsCustomerDto()
    {
        // 手机号未注册
        _customerRepoMock
            .Setup(r => r.ExistsByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _customerRepoMock
            .Setup(r => r.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _customerRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new RegisterCustomerRequest { Phone = "13800138000", VerifyCode = "1234" };
        var result = await _service.RegisterAsync(request);

        // 验证AddAsync和SaveChangesAsync被调用
        _customerRepoMock.Verify(r => r.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _customerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // 注册后IsRegistered应为true
        result.IsRegistered.Should().BeTrue();

        // 未提供昵称时，默认使用手机号后4位
        result.Nickname.Should().Be("8000");
    }

    [Fact]
    public async Task RegisterAsync_DuplicatePhone_ThrowsInvalidOperationException()
    {
        // 手机号已注册
        _customerRepoMock
            .Setup(r => r.ExistsByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new RegisterCustomerRequest { Phone = "13800138000", VerifyCode = "1234" };
        var act = async () => await _service.RegisterAsync(request);

        // 重复手机号应抛出InvalidOperationException
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("该手机号已注册");
    }

    [Fact]
    public async Task RegisterAsync_WithDeviceId_MergesOrders()
    {
        // 手机号未注册，且提供了设备ID
        _customerRepoMock
            .Setup(r => r.ExistsByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _customerRepoMock
            .Setup(r => r.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _customerRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _customerRepoMock
            .Setup(r => r.MergeOrdersToCustomerAsync(It.IsAny<Guid>(), "device-123", It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        var request = new RegisterCustomerRequest
        {
            Phone = "13800138000",
            VerifyCode = "1234",
            DeviceId = "device-123"
        };

        var result = await _service.RegisterAsync(request);

        // 提供了设备ID时，应调用合并订单方法
        _customerRepoMock.Verify(
            r => r.MergeOrdersToCustomerAsync(It.IsAny<Guid>(), "device-123", It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_WithoutDeviceId_DoesNotMerge()
    {
        // 手机号未注册，未提供设备ID
        _customerRepoMock
            .Setup(r => r.ExistsByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _customerRepoMock
            .Setup(r => r.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _customerRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new RegisterCustomerRequest
        {
            Phone = "13800138000",
            VerifyCode = "1234"
        };

        var result = await _service.RegisterAsync(request);

        // 未提供设备ID时，不应调用合并订单方法
        _customerRepoMock.Verify(
            r => r.MergeOrdersToCustomerAsync(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_RegisteredCustomer_ReturnsCustomerDto()
    {
        var customer = CreateSampleCustomer(isRegistered: true);

        _customerRepoMock
            .Setup(r => r.GetByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _customerRepoMock
            .Setup(r => r.Update(It.IsAny<CustomerEntity>()))
            .Callback<CustomerEntity>(c => c.UpdatedAt = DateTime.UtcNow);

        _customerRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new LoginCustomerRequest { Phone = "13800138000", VerifyCode = "1234" };
        var result = await _service.LoginAsync(request);

        // 验证Update和SaveChangesAsync被调用
        _customerRepoMock.Verify(r => r.Update(It.IsAny<CustomerEntity>()), Times.Once);
        _customerRepoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        // 已注册顾客登录应返回CustomerDto
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task LoginAsync_UnregisteredCustomer_ReturnsNull()
    {
        // 顾客存在但未注册（匿名用户）
        var customer = CreateSampleCustomer(isRegistered: false);

        _customerRepoMock
            .Setup(r => r.GetByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var request = new LoginCustomerRequest { Phone = "13800138000", VerifyCode = "1234" };
        var result = await _service.LoginAsync(request);

        // 未注册顾客登录应返回null
        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_PhoneNotFound_ReturnsNull()
    {
        // 手机号不存在
        _customerRepoMock
            .Setup(r => r.GetByPhoneAsync("13800138000", It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        var request = new LoginCustomerRequest { Phone = "13800138000", VerifyCode = "1234" };
        var result = await _service.LoginAsync(request);

        // 手机号不存在应返回null
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsCustomerDto()
    {
        var customer = CreateSampleCustomer();
        _customerRepoMock
            .Setup(r => r.GetByIdAsync(customer.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var result = await _service.GetByIdAsync(customer.Id);

        // 存在的ID应返回CustomerDto
        result.Should().NotBeNull();
        result!.Phone.Should().Be(customer.Phone);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        var id = Guid.NewGuid();
        _customerRepoMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CustomerEntity?)null);

        var result = await _service.GetByIdAsync(id);

        // 不存在的ID应返回null
        result.Should().BeNull();
    }

    [Fact]
    public async Task EnsureAnonymousCustomerAsync_NewDevice_CreatesCustomer()
    {
        // 设备ID没有关联的匿名顾客
        _customerRepoMock
            .Setup(r => r.GetByDeviceIdAsync("device-new", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustomerEntity>());

        _customerRepoMock
            .Setup(r => r.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _customerRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.EnsureAnonymousCustomerAsync("device-new");

        // 新设备应创建匿名顾客
        _customerRepoMock.Verify(r => r.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()), Times.Once);

        // 匿名顾客IsRegistered应为false
        result.IsRegistered.Should().BeFalse();
    }

    [Fact]
    public async Task EnsureAnonymousCustomerAsync_ExistingDevice_ReturnsExisting()
    {
        // 设备ID已关联匿名顾客
        var existingCustomer = CreateSampleCustomer(isRegistered: false, deviceId: "device-existing");

        _customerRepoMock
            .Setup(r => r.GetByDeviceIdAsync("device-existing", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<CustomerEntity> { existingCustomer });

        var result = await _service.EnsureAnonymousCustomerAsync("device-existing");

        // 已有匿名顾客时，不应再创建新的
        _customerRepoMock.Verify(r => r.AddAsync(It.IsAny<CustomerEntity>(), It.IsAny<CancellationToken>()), Times.Never);

        // 返回已有的匿名顾客
        result.Id.Should().Be(existingCustomer.Id);
    }

    [Fact]
    public async Task MergeDataAsync_MergesOrdersAndReturnsResult()
    {
        var customerId = Guid.NewGuid();
        var deviceId = "device-merge";

        // 模拟合并预览数据
        var summaries = new List<StoreMergeSummaryDto>
        {
            new() { StoreId = Guid.NewGuid(), StoreName = "店铺A", ActiveOrders = 2, CompletedOrders = 1, CartItems = 0 }
        };

        _customerRepoMock
            .Setup(r => r.GetMergePreviewAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

        // 模拟合并订单返回3条
        _customerRepoMock
            .Setup(r => r.MergeOrdersToCustomerAsync(customerId, deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(3);

        _customerRepoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.MergeDataAsync(customerId, deviceId);

        // 验证合并结果
        result.NeedMerge.Should().BeTrue();
        result.OrdersMerged.Should().Be(3);
        result.StoreSummaries.Should().HaveCount(1);

        // 验证MergeOrdersToCustomerAsync被调用
        _customerRepoMock.Verify(
            r => r.MergeOrdersToCustomerAsync(customerId, deviceId, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetMergePreviewAsync_ReturnsPreviewData()
    {
        var deviceId = "device-preview";

        var summaries = new List<StoreMergeSummaryDto>
        {
            new() { StoreId = Guid.NewGuid(), StoreName = "店铺B", ActiveOrders = 1, CompletedOrders = 0, CartItems = 2 }
        };

        _customerRepoMock
            .Setup(r => r.GetMergePreviewAsync(deviceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(summaries);

        var result = await _service.GetMergePreviewAsync(deviceId);

        // 验证预览数据
        result.NeedMerge.Should().BeTrue();
        result.StoreSummaries.Should().HaveCount(1);
        result.StoreSummaries[0].StoreName.Should().Be("店铺B");

        // 预览时不应有实际合并
        result.OrdersMerged.Should().Be(0);
    }
}