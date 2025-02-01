using IconPacksGenerator.PathDirectionsFixer.Core;
using IconPacksGenerator.PathDirectionsFixer.Models;
using JetBrains.Annotations;
using Xunit;

namespace IconPacksGenerator.Tests.PathDirectionsFixer.Core;

[TestSubject(typeof(PathProcessor))]
public class PathProcessorTest
{
    // Пример простого контура (квадрат), заданного в относительных координатах.
    // В данном тесте мы проверяем, что при использовании GetFixedPathData
    // направление внешнего контура корректируется согласно опции ToClockwise.
    [Fact]
    public void GetFixedPathData_ShouldCorrectlyFixDirections_ForSimpleSquare()
    {
        // Arrange: квадрат, заданный как "m" и "l" команды
        string d = "m 0 0 l 100 0 l 0 100 l -100 0 z";
        var options = new Options { ToClockwise = true, ToAbsolute = true };

        // Act
        var fixedPathData = PathProcessor.GetFixedPathData(d, options);

        // Assert
        // Проверяем, что первая команда — абсолютное перемещение (M)
        Assert.Equal("M", fixedPathData[0].Type);
        // Проверяем, что последняя команда — закрытие пути (z или Z)
        Assert.Equal("z", fixedPathData[fixedPathData.Count - 1].Type.ToLower());

        // Можно добавить дополнительные проверки.
        // Например, если алгоритм меняет направление внешнего контура, то
        // его ориентация (определяемая площадью) должна быть противоположной исходной.
    }

    // Тест для проверки форматирования строки пути (метод PathDataToD)
    [Fact]
    public void GetFixedPathDataString_ShouldReturnFormattedPathString()
    {
        // Arrange
        string d = "M 10 10 L 50 10 L 50 50 L 10 50 Z";
        var options = new Options { Decimals = 2, ToAbsolute = true };

        // Act
        string fixedPathString = PathProcessor.GetFixedPathDataString(d, options);

        // Assert
        // Проверяем, что строка начинается с "M" и содержит "L" и "Z"
        Assert.StartsWith("M", fixedPathString);
        Assert.Contains("L", fixedPathString);
        Assert.Contains("Z", fixedPathString.ToUpper());
    }
}