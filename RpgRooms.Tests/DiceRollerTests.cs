using RpgRooms.Core.Application.Services;
using RpgRooms.Core.Domain.Entities;
using Xunit;

namespace RpgRooms.Tests;

public class DiceRollerTests
{
    [Fact]
    public void Roll_ReplacesAbilitiesAndProficiency()
    {
        var character = new Character { Str = 16, Level = 5 };
        var result = DiceRoller.Roll("1d1 + STR + PB + 2", character);
        Assert.Equal(9, result.Total);
        Assert.Contains("STR(3)", result.Detail);
        Assert.Contains("PB(3)", result.Detail);
    }

    [Fact]
    public void Roll_SupportsSubtraction()
    {
        var character = new Character { Dex = 14 };
        var result = DiceRoller.Roll("1d1 + DEX - 2", character);
        Assert.Equal(1 + 2 - 2, result.Total);
        Assert.Contains("DEX(2)", result.Detail);
    }

    [Fact]
    public void Roll_MultipleDice()
    {
        var character = new Character();
        var result = DiceRoller.Roll("2d1", character);
        Assert.Equal(2, result.Total);
        Assert.StartsWith("2d1(1,1)", result.Detail);
    }
}
