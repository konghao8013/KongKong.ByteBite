namespace ByteBite.Shared.Helpers;

/// <summary>
/// 取货码生成器。数据库保存整数码值，页面使用 Base36 展示。
/// </summary>
public static class PickupCodeGenerator
{
    public const int MinValue = 1;
    public const int MaxValue = int.MaxValue;

    public static int GenerateValue()
        => Random.Shared.Next(MinValue, MaxValue);

    public static string ToDisplayCode(int value)
        => Base36Encoder.Encode(value);

    public static int FromDisplayCode(string code)
    {
        var value = Base36Encoder.Decode(code);
        return value is >= MinValue and <= MaxValue ? (int)value : 0;
    }
}
