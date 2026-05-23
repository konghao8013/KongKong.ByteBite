using System;
using System.Collections.Generic;

namespace ByteBite.Infrastructure.Persistence.Entities;

/// <summary>
/// 系统配置表
/// </summary>
public partial class SystemConfig
{
    public Guid Id { get; set; }

    /// <summary>
    /// 配置键名
    /// </summary>
    public string ConfigKey { get; set; } = null!;

    /// <summary>
    /// 配置值
    /// </summary>
    public string ConfigValue { get; set; } = null!;

    /// <summary>
    /// 值类型：string-字符串, number-数字, boolean-布尔, json-JSON对象
    /// </summary>
    public string ConfigType { get; set; } = null!;

    /// <summary>
    /// 配置说明
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 是否公开（前端可读取的配置，如取货码长度）
    /// </summary>
    public bool IsPublic { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
