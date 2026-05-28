using ByteBite.Shared.Helpers;

namespace ByteBite.UnitTests.Current;

public class PickupCodeGeneratorTests
{
    [Theory]
    [InlineData(1, "000001")]
    [InlineData(35, "00000Z")]
    [InlineData(36, "000010")]
    [InlineData(1295, "0000ZZ")]
    public void ToDisplayCode_UsesSixCharacterBase36(int value, string expected)
    {
        PickupCodeGenerator.ToDisplayCode(value).Should().Be(expected);
    }

    [Theory]
    [InlineData("000001", 1)]
    [InlineData("00000Z", 35)]
    [InlineData("000010", 36)]
    public void FromDisplayCode_ParsesBase36PickupCode(string code, int expected)
    {
        PickupCodeGenerator.FromDisplayCode(code).Should().Be(expected);
    }

    [Fact]
    public void GenerateValue_StaysInsideSixCharacterBase36Range()
    {
        var value = PickupCodeGenerator.GenerateValue();
        value.Should().BeGreaterThan(0);
        PickupCodeGenerator.ToDisplayCode(value).Should().HaveLength(6);
    }
}
