namespace ByteBite.Shared.Helpers;

/// <summary>
/// 取货码生成器 - 生成排除易混淆字符的随机取货码
/// </summary>
public static class PickupCodeGenerator
{
    /// <summary>
    /// 可用字符池：A-Z（排除O,I,L）+ 2-9（排除0,1）
    /// </summary>
    private const string CharSet = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    private static readonly Random _random = new();

    /// <summary>
    /// 生成随机取货码
    /// </summary>
    /// <param name="length">取货码长度，默认5位</param>
    /// <returns>随机取货码字符串</returns>
    public static string Generate(int length = 5)
    {
        if (length < 4) length = 4;
        if (length > 6) length = 6;

        return new string(Enumerable.Range(0, length)
            .Select(_ => CharSet[_random.Next(CharSet.Length)])
            .ToArray());
    }
}
