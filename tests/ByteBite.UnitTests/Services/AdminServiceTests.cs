using ByteBite.Application.DTOs.Admin;
using ByteBite.Application.Interfaces;
using ByteBite.Application.Services;
using ByteBite.Shared.Helpers;

namespace ByteBite.UnitTests.Services;

/// <summary>
/// 管理员服务单元测试 - 覆盖登录验证、商家审核、配置更新、平台统计
/// </summary>
public class AdminServiceTests
{
    private readonly Mock<IAdminRepository> _repoMock;
    private readonly AdminService _service;

    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _merchantId = Guid.NewGuid();
    private readonly string _passwordHash;

    public AdminServiceTests()
    {
        _repoMock = new Mock<IAdminRepository>();
        _service = new AdminService(_repoMock.Object);

        // 预计算BCrypt哈希，用于登录密码验证测试
        _passwordHash = PasswordHasher.HashPassword("admin123");
    }

    /// <summary>
    /// 创建活跃状态的管理员实体
    /// </summary>
    private AdminEntity CreateActiveAdmin()
    {
        return new AdminEntity
        {
            Id = _adminId,
            Username = "admin",
            PasswordHash = _passwordHash,
            DisplayName = "超级管理员",
            Role = "super_admin",
            Status = "active",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建指定状态的商家实体
    /// </summary>
    private MerchantManageEntity CreateMerchant(string status = "pending")
    {
        return new MerchantManageEntity
        {
            Id = _merchantId,
            Phone = "13800000000",
            Nickname = "测试商家",
            Status = status,
            StoreCount = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建系统配置实体
    /// </summary>
    private SystemConfigEntity CreateConfig()
    {
        return new SystemConfigEntity
        {
            Id = Guid.NewGuid(),
            ConfigKey = "platform_name",
            ConfigValue = "ByteBite",
            ConfigType = "string",
            Description = "平台名称",
            IsPublic = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 设置登录成功的通用Mock
    /// </summary>
    private void SetupLoginSuccessMocks(AdminEntity admin)
    {
        _repoMock
            .Setup(r => r.GetAdminByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        _repoMock
            .Setup(r => r.UpdateAdmin(admin));

        _repoMock
            .Setup(r => r.AddOperationLogAsync(It.IsAny<AdminOperationLogEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    /// <summary>
    /// 设置商家审核的通用Mock
    /// </summary>
    private void SetupAuditMocks(MerchantManageEntity merchant)
    {
        _repoMock
            .Setup(r => r.GetMerchantByIdAsync(_merchantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(merchant);

        _repoMock
            .Setup(r => r.UpdateMerchantStatus(merchant));

        _repoMock
            .Setup(r => r.AddAuditLogAsync(It.IsAny<MerchantAuditLogEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repoMock
            .Setup(r => r.AddOperationLogAsync(It.IsAny<AdminOperationLogEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
    }

    [Fact]
    public async Task LoginAsync_ValidCredentials_ReturnsAdminDto()
    {
        // 正确的用户名和密码应返回管理员DTO
        var admin = CreateActiveAdmin();
        SetupLoginSuccessMocks(admin);

        var request = new LoginAdminRequest { Username = "admin", Password = "admin123" };

        var result = await _service.LoginAsync(request);

        result.Should().NotBeNull();
        result!.Username.Should().Be("admin");
        result.Role.Should().Be("super_admin");

        _repoMock.Verify(r => r.UpdateAdmin(admin), Times.Once);
        _repoMock.Verify(r => r.AddOperationLogAsync(It.IsAny<AdminOperationLogEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_AdminNotFound_ReturnsNull()
    {
        // 用户名不存在时应返回null
        _repoMock
            .Setup(r => r.GetAdminByUsernameAsync("unknown", It.IsAny<CancellationToken>()))
            .ReturnsAsync((AdminEntity?)null);

        var request = new LoginAdminRequest { Username = "unknown", Password = "admin123" };

        var result = await _service.LoginAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_AdminDisabled_ReturnsNull()
    {
        // 被禁用的管理员登录时应返回null
        var admin = CreateActiveAdmin();
        admin.Status = "disabled";

        _repoMock
            .Setup(r => r.GetAdminByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        var request = new LoginAdminRequest { Username = "admin", Password = "admin123" };

        var result = await _service.LoginAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task LoginAsync_InvalidPassword_ReturnsNull()
    {
        // 错误的密码应返回null
        var admin = CreateActiveAdmin();
        _repoMock
            .Setup(r => r.GetAdminByUsernameAsync("admin", It.IsAny<CancellationToken>()))
            .ReturnsAsync(admin);

        var request = new LoginAdminRequest { Username = "admin", Password = "wrong_password" };

        var result = await _service.LoginAsync(request);

        result.Should().BeNull();
    }

    [Fact]
    public async Task AuditMerchantAsync_Approve_ChangesStatusToActive()
    {
        // 审核通过后商家状态应变为active
        var merchant = CreateMerchant(status: "pending");
        SetupAuditMocks(merchant);

        var request = new MerchantAuditRequest { Action = "approve" };

        var result = await _service.AuditMerchantAsync(_merchantId, _adminId, request);

        result.Status.Should().Be("active");
    }

    [Fact]
    public async Task AuditMerchantAsync_Reject_ChangesStatusToRejected()
    {
        // 审核拒绝后商家状态应变为rejected
        var merchant = CreateMerchant(status: "pending");
        SetupAuditMocks(merchant);

        var request = new MerchantAuditRequest { Action = "reject", Reason = "资质不符" };

        var result = await _service.AuditMerchantAsync(_merchantId, _adminId, request);

        result.Status.Should().Be("rejected");
    }

    [Fact]
    public async Task AuditMerchantAsync_Disable_ChangesStatusToDisabled()
    {
        // 禁用商家后状态应变为disabled
        var merchant = CreateMerchant(status: "active");
        SetupAuditMocks(merchant);

        var request = new MerchantAuditRequest { Action = "disable", Reason = "违规经营" };

        var result = await _service.AuditMerchantAsync(_merchantId, _adminId, request);

        result.Status.Should().Be("disabled");
    }

    [Fact]
    public async Task AuditMerchantAsync_Enable_ChangesStatusToActive()
    {
        // 解禁商家后状态应变为active
        var merchant = CreateMerchant(status: "disabled");
        SetupAuditMocks(merchant);

        var request = new MerchantAuditRequest { Action = "enable" };

        var result = await _service.AuditMerchantAsync(_merchantId, _adminId, request);

        result.Status.Should().Be("active");
    }

    [Fact]
    public async Task AuditMerchantAsync_RejectWithoutReason_ThrowsInvalidOperationException()
    {
        // 拒绝操作未填写原因时应抛出异常
        var merchant = CreateMerchant(status: "pending");
        SetupAuditMocks(merchant);

        var request = new MerchantAuditRequest { Action = "reject" };

        var act = () => _service.AuditMerchantAsync(_merchantId, _adminId, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("拒绝或禁用操作必须填写原因");
    }

    [Fact]
    public async Task AuditMerchantAsync_DisableWithoutReason_ThrowsInvalidOperationException()
    {
        // 禁用操作未填写原因时应抛出异常
        var merchant = CreateMerchant(status: "active");
        SetupAuditMocks(merchant);

        var request = new MerchantAuditRequest { Action = "disable" };

        var act = () => _service.AuditMerchantAsync(_merchantId, _adminId, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("拒绝或禁用操作必须填写原因");
    }

    [Fact]
    public async Task AuditMerchantAsync_InvalidAction_ThrowsInvalidOperationException()
    {
        // 无效的审核操作应抛出异常
        var request = new MerchantAuditRequest { Action = "invalid_action" };

        var act = () => _service.AuditMerchantAsync(_merchantId, _adminId, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("无效的审核操作：invalid_action");
    }

    [Fact]
    public async Task AuditMerchantAsync_MerchantNotFound_ThrowsInvalidOperationException()
    {
        // 商家不存在时应抛出异常
        _repoMock
            .Setup(r => r.GetMerchantByIdAsync(_merchantId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((MerchantManageEntity?)null);

        var request = new MerchantAuditRequest { Action = "approve" };

        var act = () => _service.AuditMerchantAsync(_merchantId, _adminId, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("商家不存在");
    }

    [Fact]
    public async Task UpdateConfigAsync_ExistingConfig_UpdatesValue()
    {
        // 更新已存在的配置项应修改其值
        var config = CreateConfig();
        _repoMock
            .Setup(r => r.GetConfigByIdAsync(config.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(config);
        _repoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var request = new UpdateConfigRequest { ConfigValue = "NewPlatformName" };

        var result = await _service.UpdateConfigAsync(config.Id, request);

        result.ConfigValue.Should().Be("NewPlatformName");
    }

    [Fact]
    public async Task UpdateConfigAsync_NonExistingConfig_ThrowsInvalidOperationException()
    {
        // 更新不存在的配置项应抛出异常
        var configId = Guid.NewGuid();
        _repoMock
            .Setup(r => r.GetConfigByIdAsync(configId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SystemConfigEntity?)null);

        var request = new UpdateConfigRequest { ConfigValue = "value" };

        var act = () => _service.UpdateConfigAsync(configId, request);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("配置项不存在");
    }

    [Fact]
    public async Task GetPlatformStatsAsync_ReturnsStats()
    {
        // 平台统计应正确聚合各维度数据
        var todayStats = new PlatformDailyStatEntity
        {
            Id = Guid.NewGuid(),
            StatDate = DateOnly.FromDateTime(DateTime.UtcNow),
            TotalOrders = 100,
            TotalRevenue = 5000m,
            TemplateUsageCount = 20,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _repoMock
            .Setup(r => r.GetTodayStatsAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(todayStats);
        _repoMock
            .Setup(r => r.GetTotalMerchantCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(50);
        _repoMock
            .Setup(r => r.GetActiveMerchantCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(40);
        _repoMock
            .Setup(r => r.GetPendingMerchantCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(5);
        _repoMock
            .Setup(r => r.GetTotalStoreCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(60);
        _repoMock
            .Setup(r => r.GetTotalCustomerCountAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(200);

        var result = await _service.GetPlatformStatsAsync();

        result.TotalMerchants.Should().Be(50);
        result.ActiveMerchants.Should().Be(40);
        result.PendingMerchants.Should().Be(5);
        result.TotalStores.Should().Be(60);
        result.TodayOrders.Should().Be(100);
        result.TodayRevenue.Should().Be(5000m);
        result.TotalCustomers.Should().Be(200);
        result.TemplateUsageCount.Should().Be(20);
    }
}