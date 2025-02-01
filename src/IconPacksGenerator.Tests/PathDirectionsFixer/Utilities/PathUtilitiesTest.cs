using System.Collections.Generic;
using IconPacksGenerator.PathDirectionsFixer.Models;
using IconPacksGenerator.PathDirectionsFixer.Utilities;
using JetBrains.Annotations;
using Xunit;

namespace IconPacksGenerator.Tests.PathDirectionsFixer.Utilities;

[TestSubject(typeof(PathUtilities))]
public class PathUtilitiesTest
{
    [Fact]
    public void ReversePathData_ShouldReverseClosedPathCorrectly()
    {
        // Arrange: создаем простой закрытый треугольник
        var pathData = new List<PathCommand>
        {
            new PathCommand { Type = "M", Values = new List<float> { 0, 0 } },
            new PathCommand { Type = "L", Values = new List<float> { 100, 0 } },
            new PathCommand { Type = "L", Values = new List<float> { 50, 100 } },
            new PathCommand { Type = "Z", Values = new List<float>() }
        };

        // Act: реверсируем путь
        var reversed = PathUtilities.ReversePathData(pathData);

        // Assert:
        // 1. Первый элемент в реверсированном пути — должен быть командой "M"
        Assert.Equal("M", reversed[0].Type);
        // 2. Последняя команда — должна быть командой закрытия "Z"
        Assert.Equal("z", reversed[reversed.Count - 1].Type.ToLower());
        // 3. Дополнительно можно проверить, что последовательность координат изменилась ожидаемым образом
    }

    [Fact]
    public void SplitSubpaths_ShouldReturnMultipleSubpaths_WhenMultipleMCommandsExist()
    {
        // Arrange: создаем путь с двумя подпутями
        var pathData = new List<PathCommand>
        {
            new PathCommand { Type = "M", Values = new List<float> { 0, 0 } },
            new PathCommand { Type = "L", Values = new List<float> { 50, 0 } },
            new PathCommand { Type = "M", Values = new List<float> { 100, 100 } },
            new PathCommand { Type = "L", Values = new List<float> { 150, 100 } }
        };

        // Act: разбиваем путь на подпути
        var subPaths = PathUtilities.SplitSubpaths(pathData);

        // Assert:
        Assert.Equal(2, subPaths.Count);
        Assert.Equal("M", subPaths[0][0].Type);
        Assert.Equal("M", subPaths[1][0].Type);
    }
}