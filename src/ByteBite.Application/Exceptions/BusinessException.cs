namespace ByteBite.Application.Exceptions;

/// <summary>
/// 业务异常 - 用于Service层抛出业务逻辑错误，由全局过滤器捕获并包装为统一响应
/// </summary>
public class BusinessException : Exception
{
    /// <summary>业务错误码</summary>
    public int Code { get; }

    /// <summary>初始化业务异常</summary>
    public BusinessException(int code, string message) : base(message)
    {
        Code = code;
    }

    /// <summary>初始化业务异常（默认400错误码）</summary>
    public BusinessException(string message) : base(message)
    {
        Code = 400;
    }
}
