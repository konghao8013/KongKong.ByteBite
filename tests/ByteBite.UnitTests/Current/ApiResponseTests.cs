using ByteBite.Application.DTOs.Common;

namespace ByteBite.UnitTests.Current;

public class ApiResponseTests
{
    [Fact]
    public void Success_WrapsDataWithCode200()
    {
        var response = ApiResponse.Success(new { Name = "ByteBite" });

        response.Code.Should().Be(200);
        response.Message.Should().Be("Success");
        response.Data.Should().NotBeNull();
    }

    [Fact]
    public void Fail_KeepsCodeMessageAndDetail()
    {
        var response = ApiResponse.Fail(400, "参数错误", "detail");

        response.Code.Should().Be(400);
        response.Message.Should().Be("参数错误");
        response.Detail.Should().Be("detail");
        response.Data.Should().BeNull();
    }
}
