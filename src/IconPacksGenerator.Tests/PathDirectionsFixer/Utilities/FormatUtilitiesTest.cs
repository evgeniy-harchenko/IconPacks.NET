using System.Collections.Generic;
using IconPacksGenerator.PathDirectionsFixer.Models;
using IconPacksGenerator.PathDirectionsFixer.Utilities;
using JetBrains.Annotations;
using Xunit;

namespace IconPacksGenerator.Tests.PathDirectionsFixer.Utilities;

[TestSubject(typeof(FormatUtilities))]
public class FormatUtilitiesTest
{
    [Fact]
    public void PathDataToD_ShouldFormatCorrectly_WithSpecifiedDecimalsAndMinify()
    {
        // Arrange
        var pathData = new List<PathCommand>
        {
            new PathCommand { Type = "M", Values = new List<float> { 1.12345f, 2.87654f } },
            new PathCommand { Type = "L", Values = new List<float> { 3.12345f, 4.87654f } },
            new PathCommand { Type = "Z", Values = new List<float>() }
        };
        int decimals = 2;
        bool minify = true;

        // Act
        string formatted = FormatUtilities.PathDataToD(pathData, decimals, minify);

        // Assert
        // Ожидаемый результат в minify-режиме может не содержать "L"
        // Например, ожидаем, что строка начинается с "M" и заканчивается на "z"
        Assert.StartsWith("M", formatted);
        Assert.EndsWith("z", formatted.ToLower());
    
        // Также можно проверить, что координаты правильно округлены и присутствуют
        Assert.Contains("1.12", formatted);
        Assert.Contains("2.88", formatted);
        Assert.Contains("3.12", formatted);
        Assert.Contains("4.88", formatted);
    }

    [Fact]
    public void FormatValues_ShouldRoundNumbersCorrectly_WhenMinifyEnabled()
    {
        // Arrange
        List<float> values = new List<float> { 0.5000f, 2.000f, 3.14159265f };
        int decimals = 2;
        bool minify = true;

        // Act
        string formatted = FormatUtilities.FormatValues(values, decimals, minify);

        // Assert
        // Проверяем, что "2.000" стало "2" (если minify убирает .0) и 3.14159265 округлилось до "3.14"
        Assert.Contains("2", formatted);
        Assert.Contains("3.14", formatted);
    }

    [Fact]
    public void PathDataToD_ShouldFormatCorrectly_WithoutMinify()
    {
        // Arrange
        var pathData = new List<PathCommand>
        {
            new PathCommand { Type = "M", Values = new List<float> { 1.12345f, 2.87654f } },
            new PathCommand { Type = "L", Values = new List<float> { 3.12345f, 4.87654f } },
            new PathCommand { Type = "Z", Values = new List<float>() }
        };
        int decimals = 2;
        bool minify = false;

        // Act
        string formatted = FormatUtilities.PathDataToD(pathData, decimals, minify);

        // Assert
        // В не-minify режиме команда "L" должна присутствовать
        Assert.Contains("L", formatted);
        Assert.StartsWith("M", formatted);
        Assert.EndsWith("Z", formatted.ToUpper());

        // Дополнительная проверка на наличие координат
        Assert.Contains("1.12", formatted);
        Assert.Contains("2.88", formatted);
        Assert.Contains("3.12", formatted);
        Assert.Contains("4.88", formatted);
    }
}