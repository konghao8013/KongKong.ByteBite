using ByteBite.Api.Filters;
using ByteBite.Application.DTOs.Common;
using ByteBite.Application.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace ByteBite.IntegrationTests.Current;

public class ApiResponseWrapperFilterTests
{
    [Fact]
    public void OnResultExecuting_ReplacesDeclaredTypeAfterWrappingDto()
    {
        var filter = new ApiResponseWrapperFilter();
        var result = new ObjectResult(new CustomerCartStoreDto())
        {
            DeclaredType = typeof(CustomerCartStoreDto)
        };
        var context = new ResultExecutingContext(
            new ActionContext(new DefaultHttpContext(), new RouteData(), new ActionDescriptor()),
            [],
            result,
            controller: new object());

        filter.OnResultExecuting(context);

        result.Value.Should().BeOfType<ApiResponse>();
        result.DeclaredType.Should().Be(typeof(ApiResponse));
        result.StatusCode.Should().Be(StatusCodes.Status200OK);
    }
}
