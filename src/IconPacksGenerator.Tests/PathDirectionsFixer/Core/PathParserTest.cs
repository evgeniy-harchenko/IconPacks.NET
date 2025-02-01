using System.Collections.Generic;
using IconPacksGenerator.PathDirectionsFixer.Core;
using IconPacksGenerator.PathDirectionsFixer.Models;
using JetBrains.Annotations;
using Xunit;

namespace IconPacksGenerator.Tests.PathDirectionsFixer.Core;

[TestSubject(typeof(PathParser))]
public class PathParserTests
{
    [Fact]
    public void ParsePathDataNormalized_ShouldParseMoveToCorrectly()
    {
        // Arrange
        string d = "M 10 20 L 30 40";

        // Act
        var result = PathParser.ParsePathDataNormalized(d);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("M", result[0].Type);
        Assert.Equal(new List<float> { 10, 20 }, result[0].Values);

        Assert.Equal("L", result[1].Type);
        Assert.Equal(new List<float> { 30, 40 }, result[1].Values);
    }

    [Fact]
    public void ParsePathDataNormalized_ShouldHandleRelativeMoveTo()
    {
        // Arrange
        string d = "m 10 20 l 30 40";

        // Act
        var result = PathParser.ParsePathDataNormalized(d);

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal("M", result[0].Type); // Должно быть преобразовано в абсолютное
        Assert.Equal("L", result[1].Type);

        Assert.Equal(new List<float> { 10, 20 }, result[0].Values);
        Assert.Equal(new List<float> { 40, 60 }, result[1].Values); // (10+30, 20+40)
    }

    [Theory]
    [InlineData("H 100", "L", 100, 0)]
    [InlineData("h 100", "L", 100, 0)]
    [InlineData("V 200", "L", 0, 200)]
    [InlineData("v 200", "L", 0, 200)]
    public void ParsePathDataNormalized_ShouldConvertShortCommandsToLonghand(string d, string expectedType, float x,
        float y)
    {
        // Act
        var result = PathParser.ParsePathDataNormalized($"M 0 0 {d}");

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal(expectedType, result[1].Type);
        Assert.Equal(new List<float> { x, y }, result[1].Values);
    }

    [Fact]
    public void ParsePathDataNormalized_ShouldParseCurveCommands()
    {
        // Arrange
        string d = "M 10 10 C 20 20, 30 30, 40 40";

        // Act
        var result = PathParser.ParsePathDataNormalized(d);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("C", result[1].Type);
        Assert.Equal(new List<float> { 20, 20, 30, 30, 40, 40 }, result[1].Values);
    }

    [Fact]
    public void ParsePathDataNormalized_ShouldHandleArcsCorrectly()
    {
        // Arrange
        string d = "M 100 100 A 50 50 0 1 1 150 150";

        // Act
        var result = PathParser.ParsePathDataNormalized(d);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("A", result[1].Type);
        Assert.Equal(new List<float> { 50, 50, 0, 1, 1, 150, 150 }, result[1].Values);
    }

    [Fact]
    public void ParsePathDataNormalized_ShouldHandleClosePathCommand()
    {
        // Arrange
        string d = "M 10 10 L 20 20 Z";

        // Act
        var result = PathParser.ParsePathDataNormalized(d);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Z", result[2].Type);
    }

    [Fact]
    public void ParsePathDataNormalized_ShouldHandleMultipleCommands()
    {
        // Arrange
        string d = "M 0 0 L 10 10 C 20 20, 30 30, 40 40 S 50 50, 60 60 Q 70 70, 80 80 T 90 90";

        // Act
        var result = PathParser.ParsePathDataNormalized(d);

        // Assert
        Assert.Equal(6, result.Count);

        Assert.Equal("M", result[0].Type);
        Assert.Equal("L", result[1].Type);
        Assert.Equal("C", result[2].Type);
        Assert.Equal("C", result[3].Type); // S -> C
        Assert.Equal("Q", result[4].Type);
        Assert.Equal("Q", result[5].Type); // T -> Q
    }

    [Fact]
    public void ParsePathDataNormalized_ShouldHandleOptionsCorrectly()
    {
        // Arrange
        string d = "m 10 20 l 30 40";
        var options = new Options { ToAbsolute = false };

        // Act
        var result = PathParser.ParsePathDataNormalized(d, options);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("m", result[0].Type); // Должно остаться в относительном виде
        Assert.Equal("l", result[1].Type);
    }

    [Fact]
    public void ParsePathDataNormalized_ShouldRespectToAbsoluteOption()
    {
        // Arrange
        string d = "m 10 20 l 30 40";
        var options = new Options { ToAbsolute = false };

        // Act
        var result = PathParser.ParsePathDataNormalized(d, options);

        // Assert
        Assert.Equal(2, result.Count);

        // Должны остаться относительными!
        Assert.Equal("m", result[0].Type);
        Assert.Equal("l", result[1].Type);
    }

    [Fact]
    public void ParsePathDataNormalized_ShouldConvertToAbsoluteWhenEnabled()
    {
        // Arrange
        string d = "m 10 20 l 30 40";
        var options = new Options { ToAbsolute = true };

        // Act
        var result = PathParser.ParsePathDataNormalized(d, options);

        // Assert
        Assert.Equal(2, result.Count);

        // Должны стать абсолютными!
        Assert.Equal("M", result[0].Type);
        Assert.Equal("L", result[1].Type);
    }
}