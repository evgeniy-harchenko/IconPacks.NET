using System.Collections.Generic;
using System.Drawing;
using IconPacksGenerator.PathDirectionsFixer.Models;
using IconPacksGenerator.PathDirectionsFixer.Utilities;
using JetBrains.Annotations;
using Xunit;

namespace IconPacksGenerator.Tests.PathDirectionsFixer.Utilities;

[TestSubject(typeof(CurveUtilities))]
public class CurveUtilitiesTest
{
    [Fact]
    public void Quadratic2Cubic_ShouldConvertQuadraticToCubicCorrectly()
    {
        // Arrange
        // Исходная квадратичная кривая определяется точкой начала p0, контрольной точкой и конечной точкой.
        // Пусть p0 = (0,0), контрольная точка = (50, 100) и конечная точка = (100, 0)
        var p0 = new PointF(0, 0);
        List<float> quadValues = new List<float> { 50, 100, 100, 0 };

        // Act
        PathCommand cubic = CurveUtilities.Quadratic2Cubic(p0, quadValues);

        // Assert
        // Мы ожидаем, что тип команды изменится на "C" (кубическая кривая)
        Assert.Equal("C", cubic.Type);
        // Проверяем, что итоговый массив значений имеет 6 чисел
        Assert.Equal(6, cubic.Values.Count);

        // Дополнительно можно проверить, что координаты контрольных точек вычислены согласно формуле:
        // cp1 = p0 + 2/3 * (quadCP - p0)
        // cp2 = quadEnd + 2/3 * (quadCP - quadEnd)
        float expectedCp1X = 0 + 2f / 3f * (50 - 0);
        float expectedCp1Y = 0 + 2f / 3f * (100 - 0);
        float expectedCp2X = 100 + 2f / 3f * (50 - 100);
        float expectedCp2Y = 0 + 2f / 3f * (100 - 0);

        Assert.Equal(expectedCp1X, cubic.Values[0], 6);
        Assert.Equal(expectedCp1Y, cubic.Values[1], 6);
        Assert.Equal(expectedCp2X, cubic.Values[2], 6);
        Assert.Equal(expectedCp2Y, cubic.Values[3], 6);
        // Конечная точка должна совпадать с (100, 0)
        Assert.Equal(100, cubic.Values[4], 6);
        Assert.Equal(0, cubic.Values[5], 6);
    }

    [Fact]
    public void ArcToBezier_ShouldReturnBezierSegments_ForSimpleArc()
    {
        // Arrange
        // Пример: дуга с центром между точками p0 и конечной точкой
        // Пусть p0 = (100, 100), и мы рисуем дугу с радиусами 50,50 до точки (150,150)
        var p0 = new PointF(100, 100);
        // Формат A: rx, ry, rotation, largeArcFlag, sweepFlag, x, y
        List<float> arcValues = new List<float> { 50, 50, 0, 0, 1, 150, 150 };

        // Act
        List<PathCommand> bezierSegments = CurveUtilities.ArcToBezier(p0, arcValues, splitSegments: 1);

        // Assert
        // В зависимости от угла дуги может получиться 1 или несколько сегментов.
        // Здесь проверим, что список не пустой и что тип команды каждого сегмента равен "C".
        Assert.NotEmpty(bezierSegments);
        foreach (var seg in bezierSegments)
        {
            Assert.Equal("C", seg.Type);
            // Каждый сегмент должен иметь 6 координат (3 точки по 2 координаты)
            Assert.Equal(6, seg.Values.Count);
        }
    }
}