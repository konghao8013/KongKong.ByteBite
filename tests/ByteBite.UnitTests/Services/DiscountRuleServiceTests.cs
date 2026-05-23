using ByteBite.Application.DTOs.Discount;
using ByteBite.Application.Interfaces;
using ByteBite.Application.Services;

namespace ByteBite.UnitTests.Services;

/// <summary>
/// 优惠活动服务单元测试 - 覆盖增删改查、状态切换
/// </summary>
public class DiscountRuleServiceTests
{
    private readonly Mock<IDiscountRuleRepository> _repoMock;
    private readonly DiscountRuleService _service;

    private readonly Guid _storeId = Guid.NewGuid();
    private readonly Guid _ruleId = Guid.NewGuid();

    public DiscountRuleServiceTests()
    {
        _repoMock = new Mock<IDiscountRuleRepository>();
        _service = new DiscountRuleService(_repoMock.Object);
    }

    /// <summary>
    /// 创建示例优惠活动实体
    /// </summary>
    private DiscountRuleEntity CreateSampleRule(string status = "active")
    {
        return new DiscountRuleEntity
        {
            Id = _ruleId,
            StoreId = _storeId,
            Name = "满20减5",
            DiscountType = "full_reduction",
            ThresholdAmount = 20m,
            DiscountAmount = 5m,
            ApplyScope = "all",
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(7),
            Status = status,
            UsedCount = 0,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// 创建优惠活动请求DTO
    /// </summary>
    private static CreateDiscountRuleRequest CreateRequest()
    {
        return new CreateDiscountRuleRequest
        {
            Name = "满20减5",
            DiscountType = "full_reduction",
            ThresholdAmount = 20m,
            DiscountAmount = 5m,
            ApplyScope = "all",
            StartTime = DateTime.UtcNow.AddDays(-1),
            EndTime = DateTime.UtcNow.AddDays(7)
        };
    }

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsDiscountRuleDto()
    {
        // 创建有效优惠活动应返回正确的DTO
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<DiscountRuleEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _repoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(_storeId, CreateRequest());

        result.Should().NotBeNull();
        result.Status.Should().Be("active");
        result.UsedCount.Should().Be(0);
        result.StoreId.Should().Be(_storeId);
        result.Name.Should().Be("满20减5");

        _repoMock.Verify(r => r.AddAsync(It.IsAny<DiscountRuleEntity>(), It.IsAny<CancellationToken>()), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_ExistingRule_UpdatesName()
    {
        // 更新已存在的优惠活动名称应成功
        var rule = CreateSampleRule();
        _repoMock
            .Setup(r => r.GetByIdAsync(_ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);
        _repoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var updateRequest = new UpdateDiscountRuleRequest { Name = "满30减8" };

        var result = await _service.UpdateAsync(_ruleId, updateRequest);

        result.Should().NotBeNull();
        result.Name.Should().Be("满30减8");
    }

    [Fact]
    public async Task UpdateAsync_NonExistingRule_ThrowsKeyNotFoundException()
    {
        // 更新不存在的优惠活动应抛出异常
        _repoMock
            .Setup(r => r.GetByIdAsync(_ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscountRuleEntity?)null);

        var updateRequest = new UpdateDiscountRuleRequest { Name = "新名称" };

        var act = () => _service.UpdateAsync(_ruleId, updateRequest);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("优惠活动不存在");
    }

    [Fact]
    public async Task DeleteAsync_ExistingRule_SetsDeletedAt()
    {
        // 删除已存在的优惠活动应设置DeletedAt时间戳
        var rule = CreateSampleRule();
        _repoMock
            .Setup(r => r.GetByIdAsync(_ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);
        _repoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _service.DeleteAsync(_ruleId);

        rule.DeletedAt.Should().NotBeNull();
        _repoMock.Verify(r => r.Update(rule), Times.Once);
        _repoMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistingRule_ThrowsKeyNotFoundException()
    {
        // 删除不存在的优惠活动应抛出异常
        _repoMock
            .Setup(r => r.GetByIdAsync(_ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscountRuleEntity?)null);

        var act = () => _service.DeleteAsync(_ruleId);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("优惠活动不存在");
    }

    [Fact]
    public async Task ToggleStatusAsync_ActiveRule_ChangesToInactive()
    {
        // 激活状态的优惠活动切换后应变为停用
        var rule = CreateSampleRule(status: "active");
        _repoMock
            .Setup(r => r.GetByIdAsync(_ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);
        _repoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.ToggleStatusAsync(_ruleId);

        result.Status.Should().Be("inactive");
    }

    [Fact]
    public async Task ToggleStatusAsync_InactiveRule_ChangesToActive()
    {
        // 停用状态的优惠活动切换后应变为激活
        var rule = CreateSampleRule(status: "inactive");
        _repoMock
            .Setup(r => r.GetByIdAsync(_ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);
        _repoMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _service.ToggleStatusAsync(_ruleId);

        result.Status.Should().Be("active");
    }

    [Fact]
    public async Task ToggleStatusAsync_NonExistingRule_ThrowsKeyNotFoundException()
    {
        // 切换不存在的优惠活动状态应抛出异常
        _repoMock
            .Setup(r => r.GetByIdAsync(_ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscountRuleEntity?)null);

        var act = () => _service.ToggleStatusAsync(_ruleId);

        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage("优惠活动不存在");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsDiscountRuleDto()
    {
        // 根据ID查询已存在的优惠活动应返回DTO
        var rule = CreateSampleRule();
        _repoMock
            .Setup(r => r.GetByIdAsync(_ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rule);

        var result = await _service.GetByIdAsync(_ruleId);

        result.Should().NotBeNull();
        result!.Id.Should().Be(_ruleId);
        result.Name.Should().Be("满20减5");
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // 根据ID查询不存在的优惠活动应返回null
        _repoMock
            .Setup(r => r.GetByIdAsync(_ruleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((DiscountRuleEntity?)null);

        var result = await _service.GetByIdAsync(_ruleId);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByStoreIdAsync_ReturnsListOfRules()
    {
        // 根据店铺ID查询应返回优惠活动列表
        var rules = new List<DiscountRuleEntity>
        {
            CreateSampleRule(),
            CreateSampleRule()
        };
        rules[1].Id = Guid.NewGuid();
        rules[1].Name = "8折优惠";

        _repoMock
            .Setup(r => r.GetByStoreIdAsync(_storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(rules);

        var result = await _service.GetByStoreIdAsync(_storeId);

        result.Should().HaveCount(2);
    }
}
