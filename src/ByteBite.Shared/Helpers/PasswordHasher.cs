namespace ByteBite.Shared.Helpers;

/// <summary>
/// 密码哈希工具 - 使用BCrypt加密和验证密码
/// </summary>
public static class PasswordHasher
{
    /// <summary>对密码进行BCrypt加密</summary>
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    /// <summary>验证密码是否匹配</summary>
    public static bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
