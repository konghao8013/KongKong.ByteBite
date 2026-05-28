using ByteBite.Application.DTOs.Common;

namespace ByteBite.IntegrationTests.Current;

public class ApiSurfaceBuildTests
{
    [Fact]
    public void ApiResponse_Contract_IsAvailableToIntegrationTests()
    {
        var response = ApiResponse.Success(new { Ready = true });

        response.Code.Should().Be(200);
        response.Data.Should().NotBeNull();
    }
}
