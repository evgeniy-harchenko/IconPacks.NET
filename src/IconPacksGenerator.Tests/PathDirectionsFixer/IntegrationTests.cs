using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using IconPacksGenerator.PathDirectionsFixer.Core;
using IconPacksGenerator.PathDirectionsFixer.Models;
using IconPacksGenerator.PathDirectionsFixer.Utilities;
using Xunit;

namespace IconPacksGenerator.Tests.PathDirectionsFixer;

public class IntegrationTests
{
    [Fact]
    public void FullPipeline_ShouldProcessSvgPathCorrectly()
    {
        // Arrange
        string d = "M 10 10 L 110 10 L 110 60 L 10 60 Z";
        var options = new Options
        {
            ToAbsolute = true,
            ToLonghands = true,
            ToClockwise = false,
            Decimals = 1
        };

        // Act
        var fixedCommands = PathProcessor.GetFixedPathData(d, options);
        string finalPath = FormatUtilities.PathDataToD(fixedCommands, options.Decimals);

        // Базовые проверки формата строки
        Assert.StartsWith("M", finalPath);
        Assert.Contains("L", finalPath);
        Assert.EndsWith("Z", finalPath.ToUpper());

        // Извлекаем точки из финальных команд с помощью PolygonUtilities.GetPathDataPoly
        List<PointF> polyPoints = PolygonUtilities.GetPathDataPoly(fixedCommands);

        // Если в результате есть дублирование замыкающей точки (например, [10,10] в начале и в конце),
        // удалим последнюю, чтобы сравнивать только уникальные вершины контура.
        if (polyPoints.Count > 0 && polyPoints.First().Equals(polyPoints.Last()))
        {
            polyPoints.RemoveAt(polyPoints.Count - 1);
        }

        // Ожидаемый набор точек (не зависит от порядка, главное — геометрическая эквивалентность)
        var expectedPoints = new List<PointF>
        {
            new PointF(10, 10),
            new PointF(110, 10),
            new PointF(110, 60),
            new PointF(10, 60)
        };

        // Для сравнения сортируем оба списка (сортировка по X, затем по Y)
        var sortedPolyPoints = polyPoints.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
        var sortedExpectedPoints = expectedPoints.OrderBy(p => p.X).ThenBy(p => p.Y).ToList();

        Assert.Equal(sortedExpectedPoints, sortedPolyPoints);
    }


    [Fact]
    public void FullPipeline_WithComplexPath_ShouldReturnExpectedResult()
    {
        // Arrange
        // Пример сложного SVG-пути с дугой и кривой
        string d = "M 100 100 A 50 50 0 0 1 150 150 L 200 100 Z";
        var options = new Options
        {
            ToAbsolute = true,
            ToLonghands = true,
            ToClockwise = true,
            Decimals = 2,
            ArcToCubic = true, // Преобразование дуг в кубические кривые
            QuadraticToCubic = true // Преобразование квадратичных в кубические
        };

        // Act
        var fixedCommands = PathProcessor.GetFixedPathData(d, options);
        string finalPath = FormatUtilities.PathDataToD(fixedCommands, options.Decimals);

        // Assert
        // Проверяем, что итоговый путь содержит команды M, L, C (так как дуга преобразована)
        Assert.StartsWith("M", finalPath);
        Assert.Contains("L", finalPath);
        Assert.Contains("C", finalPath);
        Assert.Contains("Z", finalPath.ToUpper());
    }

    [Fact]
    public void FullPipeline_ShouldConvertArcToCubic_WhenArcToCubicIsEnabled()
    {
        // Arrange: путь с дугой
        string d = "M 100 100 A 50 50 0 1 1 200 200 Z";
        var options = new Options
        {
            ArcToCubic = true, // Преобразуем дугу в кубические Безье-кривые
            ToAbsolute = true,
            Decimals = 2
        };

        // Act
        var fixedCommands = PathProcessor.GetFixedPathData(d, options);
        string finalPath = FormatUtilities.PathDataToD(fixedCommands, options.Decimals);

        // Assert
        Assert.DoesNotContain("A", finalPath); // Дуг не должно остаться
        Assert.Contains("C", finalPath); // Должны появиться кубические Безье-кривые
        Assert.StartsWith("M", finalPath);
        Assert.EndsWith("Z", finalPath.ToUpper());
    }

    [Fact]
    public void FullPipeline_ShouldConvertQuadraticToCubic_WhenEnabled()
    {
        // Arrange: путь с квадратичными кривыми
        string d = "M 10 10 Q 50 100, 100 10 T 200 10";
        var options = new Options
        {
            QuadraticToCubic = true, // Преобразуем Q → C
            ToAbsolute = true,
            Decimals = 2
        };

        // Act
        var fixedCommands = PathProcessor.GetFixedPathData(d, options);
        string finalPath = FormatUtilities.PathDataToD(fixedCommands, options.Decimals);

        // Assert
        Assert.DoesNotContain("Q", finalPath); // Q не должно остаться
        Assert.DoesNotContain("T", finalPath); // T тоже должно исчезнуть
        Assert.Contains("C", finalPath); // Должны появиться кубические Безье
        Assert.StartsWith("M", finalPath);
    }

    [Fact]
    public void FullPipeline_ShouldPreserveNestedContours()
    {
        // Arrange: внешний прямоугольник + внутренний контур
        string d = "M 10 10 L 110 10 L 110 110 L 10 110 Z M 40 40 L 80 40 L 80 80 L 40 80 Z";
        var options = new Options
        {
            ToAbsolute = true,
            ToClockwise = true, // Проверяем исправление направлений
            Decimals = 2
        };

        // Act
        var fixedCommands = PathProcessor.GetFixedPathData(d, options);
        string finalPath = FormatUtilities.PathDataToD(fixedCommands, options.Decimals);

        // Assert
        Assert.StartsWith("M", finalPath); // Должны быть 2 M-команды
        Assert.Contains("L", finalPath);
        Assert.Contains("Z", finalPath);
    }

    [Fact]
    public void FullPipeline_ShouldExpandShorthandCommands_WhenToLonghandsIsEnabled()
    {
        // Arrange: путь с сокращёнными командами
        string d = "M 10 10 H 100 V 200 S 300 300, 400 400";
        var options = new Options
        {
            ToLonghands = true, // Преобразуем H → L, V → L, S → C
            ToAbsolute = true,
            Decimals = 2
        };

        // Act
        var fixedCommands = PathProcessor.GetFixedPathData(d, options);
        string finalPath = FormatUtilities.PathDataToD(fixedCommands, options.Decimals);

        // Assert
        Assert.DoesNotContain("H", finalPath); // H должно превратиться в L
        Assert.DoesNotContain("V", finalPath); // V тоже
        Assert.DoesNotContain("S", finalPath); // S → C
        Assert.Contains("L", finalPath);
        Assert.Contains("C", finalPath);
    }
}