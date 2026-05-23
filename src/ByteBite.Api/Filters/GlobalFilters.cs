using ByteBite.Application.DTOs.Common;
using ByteBite.Application.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace ByteBite.Api.Filters;

/// <summary>
/// API响应包装过滤器 - 自动将控制器返回值包装为ApiResponse格式 { code, message, data }
/// </summary>
public class ApiResponseWrapperFilter : IResultFilter
{
    /// <summary>
    /// 请求结果执行前 - 将ObjectResult的Value包装为ApiResponse.Success
    /// </summary>
    public void OnResultExecuting(ResultExecutingContext context)
    {
        if (context.Result is ObjectResult objectResult)
        {
            // 已经是ApiResponse类型则不再重复包装
            if (objectResult.Value is ApiResponse) return;
            objectResult.Value = ApiResponse.Success(objectResult.Value);
            // 所有响应统一返回HTTP 200，错误信息通过ApiResponse.code传递
            objectResult.StatusCode = (int)HttpStatusCode.OK;
        }
    }

    public void OnResultExecuted(ResultExecutedContext context) { }
}

/// <summary>
/// 全局异常过滤器 - 捕获所有未处理异常，按异常类型映射为统一ApiResponse格式
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _logger;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) { _logger = logger; }

    /// <summary>
    /// 异常处理 - 根据异常类型映射错误码和消息，非业务异常通过detail字段返回详细堆栈
    /// </summary>
    public void OnException(ExceptionContext context)
    {
        var exception = context.Exception;
        // 记录完整异常日志供排查
        _logger.LogError(exception, "请求异常: {Path}", context.HttpContext.Request.Path);

        // 根据异常类型映射错误码、消息和详细信息
        var (code, message, detail) = exception switch
        {
            BusinessException bizEx => (bizEx.Code, bizEx.Message, (string?)null),
            UnauthorizedAccessException => (401, "未授权访问", (string?)null),
            KeyNotFoundException => (404, exception.Message, (string?)null),
            InvalidOperationException => (400, exception.Message, exception.StackTrace),
            // 未知异常返回500，detail包含完整异常信息供前端调试
            _ => (500, "服务器内部错误", $"{exception.GetType().Name}: {exception.Message}\n{exception.StackTrace}")
        };

        context.Result = new ObjectResult(ApiResponse.Fail(code, message, detail))
        {
            // 统一返回HTTP 200，错误信息通过ApiResponse传递
            StatusCode = (int)HttpStatusCode.OK
        };
        context.ExceptionHandled = true;
    }
}
