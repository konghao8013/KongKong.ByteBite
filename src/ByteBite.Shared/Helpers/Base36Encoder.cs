namespace ByteBite.Shared.Helpers;

public static class Base36Encoder
{
    private const string CharSet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private const int Base = 36;
    private const int CodeLength = 6;

    public static string Encode(long value)
    {
        if (value < 0) value = 0;
        var chars = new char[CodeLength];
        for (var i = CodeLength - 1; i >= 0; i--)
        {
            chars[i] = CharSet[(int)(value % Base)];
            value /= Base;
        }
        return new string(chars);
    }

    public static long Decode(string code)
    {
        if (string.IsNullOrEmpty(code)) return 0;
        long result = 0;
        foreach (var c in code.ToUpperInvariant())
        {
            var idx = CharSet.IndexOf(c);
            if (idx < 0) return 0;
            result = result * Base + idx;
        }
        return result;
    }
}
