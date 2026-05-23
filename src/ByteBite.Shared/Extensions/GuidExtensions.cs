namespace ByteBite.Shared.Extensions;

/// <summary>
/// Guid扩展方法
/// </summary>
public static class GuidExtensions
{
    /// <summary>判断Guid是否为空</summary>
    public static bool IsEmpty(this Guid guid) => guid == Guid.Empty;
}
