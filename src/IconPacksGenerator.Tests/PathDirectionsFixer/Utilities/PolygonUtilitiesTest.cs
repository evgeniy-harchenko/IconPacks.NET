using System.Collections.Generic;
using System.Drawing;
using IconPacksGenerator.PathDirectionsFixer.Models;
using IconPacksGenerator.PathDirectionsFixer.Utilities;
using JetBrains.Annotations;
using Xunit;

namespace IconPacksGenerator.Tests.PathDirectionsFixer.Utilities;

[TestSubject(typeof(PolygonUtilities))]
public class PolygonUtilitiesTest
{
    [Fact]
    public void PolygonArea_ShouldReturnCorrectArea_ForSquare()
    {
        // Arrange: квадрат 0,0 - 100,0 - 100,100 - 0,100
        var points = new List<PointF>
        {
            new PointF(0, 0),
            new PointF(100, 0),
            new PointF(100, 100),
            new PointF(0, 100)
        };

        // Act
        float area = PolygonUtilities.PolygonArea(points, absolute: true);

        // Assert
        Assert.Equal(10000, area);
    }

    [Theory]
    [InlineData(50, 50, true)] // точка внутри
    [InlineData(150, 50, false)] // точка вне
    public void IsPointInPolygon_ShouldDeterminePointCorrectly(float x, float y, bool expected)
    {
        // Arrange: тот же квадрат, что и в предыдущем тесте
        var points = new List<PointF>
        {
            new PointF(0, 0),
            new PointF(100, 0),
            new PointF(100, 100),
            new PointF(0, 100)
        };
        // Получаем ограничивающий прямоугольник
        BoundingBox bb = BoundingBox.GetPolyBBox(points);
        var pt = new PointF(x, y);

        // Act
        bool inside = PolygonUtilities.IsPointInPolygon(pt, points, bb);

        // Assert
        Assert.Equal(expected, inside);
    }

    [Fact]
    public void GetPathDataPoly_ShouldReturnCorrectPolygon_ForSimpleCommands()
    {
        // Arrange
        // Создадим путь с командами M, L и C.
        // Первая команда "M" задаёт начальную точку (10,10).
        // Вторая команда "L" задаёт конечную точку (20,20).
        // Третья команда "C" задаёт кубическую кривую с контрольными точками:
        //   - Начальная контрольная точка (30,30)
        //   - Вторая контрольная точка (40,40)
        //   - Конечная точка (50,50)
        var pathData = new List<PathCommand>
        {
            new PathCommand { Type = "M", Values = new List<float> { 10, 10 } },
            new PathCommand { Type = "L", Values = new List<float> { 20, 20 } },
            new PathCommand { Type = "C", Values = new List<float> { 30, 30, 40, 40, 50, 50 } }
        };

        // Act
        List<PointF> poly = PolygonUtilities.GetPathDataPoly(pathData);

        // Assert
        // Разберём, что должен вернуть метод:
        // 1. Для "M" команды:
        //    - После блока "M" (не попадает в switch, т.к. тип не "A", "C" или "Q"), в блоке ниже
        //      if (type.ToLower() != "z") добавится точка p, равная последним двум координатам "M", т.е. (10,10).
        // 2. Для "L" команды:
        //    - Аналогично, добавится точка (20,20).
        // 3. Для "C" команды:
        //    - В switch по типу "C": если есть cp1 (значения первые два числа), добавляем cp1 = (30,30) и
        //      затем добавляем вторую пару координат (40,40).
        //    - После switch, т.к. "C" не равно "z", блок if добавляет точку p, равную (50,50).
        // Итого ожидается последовательность точек:
        // (10,10), (20,20), (30,30), (40,40), (50,50)
        Assert.Equal(5, poly.Count);
        Assert.Equal(new PointF(10, 10), poly[0]);
        Assert.Equal(new PointF(20, 20), poly[1]);
        Assert.Equal(new PointF(30, 30), poly[2]);
        Assert.Equal(new PointF(40, 40), poly[3]);
        Assert.Equal(new PointF(50, 50), poly[4]);
    }
}