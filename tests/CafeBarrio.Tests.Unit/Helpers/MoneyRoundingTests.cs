using CafeBarrio.Application.Common.Helpers;
using FluentAssertions;
using Xunit;

namespace CafeBarrio.Tests.Unit.Helpers;

public class MoneyRoundingTests
{
    [Theory]
    [InlineData(1.235, 1.24)]
    [InlineData(1.245, 1.25)]
    [InlineData(1.005, 1.01)]
    [InlineData(1.225, 1.23)]    // ← añadir
    [InlineData(-1.235, -1.24)]  // ← añadir: negativo también AwayFromZero
    public void Round_ShouldUseAwayFromZero(decimal input, decimal expected)
    {
        var result = MoneyRounding.Round(input);
        result.Should().Be(expected);
    }
}
