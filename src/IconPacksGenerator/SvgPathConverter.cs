using IconPacksGenerator.PathDirectionsFixer.Core;
using SkiaSharp;
using Svg;
using Svg.Pathing;

namespace IconPacksGenerator;

public static class SvgPathConverter
{
    public static SKPath ConvertSvgToSinglePath(string svgFilePath)
    {
        // Загружаем SVG документ
        var svgDocument = SvgDocument.Open(svgFilePath);

        if (svgDocument == null)
        {
            throw new InvalidOperationException("SVG документ не удалось загрузить.");
        }

        // Создаем единый путь
        var combinedPath = new SKPath();

        // Рекурсивно обходим элементы
        TraverseSvgElement(svgDocument, combinedPath);

        return combinedPath;
    }

    private static void TraverseSvgElement(SvgElement element, SKPath combinedPath, SKMatrix? parentTransform = null)
    {
        var localTransform = GetTransform(element);
        var combinedTransform = parentTransform.HasValue
            ? localTransform.PreConcat(parentTransform.Value)
            : localTransform;

        SKPath? generatedPath = null;

        if (element is SvgPath svgPath)
        {
            generatedPath = new SKPath();
            using var skPath = SKPath.ParseSvgPathData(svgPath.PathData.ToString());
            //skPath.Transform(combinedTransform);

            if (svgPath.FillRule == SvgFillRule.EvenOdd)
            {
                var path = SKPath.ParseSvgPathData(PathProcessor.GetFixedPathDataString(svgPath.PathData.ToString()));
                generatedPath.AddPath(path);
            }
            else
            {
                generatedPath.AddPath(skPath);
            }
        }
        else if (element is SvgCircle svgCircle)
        {
            // Преобразуем SvgCircle в SKPath
            var cx = (float)svgCircle.CenterX;
            var cy = (float)svgCircle.CenterY;
            var r = (float)svgCircle.Radius;

            using var circlePath = new SKPath();
            circlePath.AddCircle(cx, cy, r);
            //circlePath.Transform(combinedTransform);
            generatedPath = new SKPath();
            generatedPath.AddPath(circlePath);
        }
        else if (element is SvgEllipse svgEllipse)
        {
            // Преобразуем SvgEllipse в SKPath
            var cx = (float)svgEllipse.CenterX;
            var cy = (float)svgEllipse.CenterY;
            var rx = (float)svgEllipse.RadiusX;
            var ry = (float)svgEllipse.RadiusY;

            using var ellipsePath = new SKPath();
            ellipsePath.AddOval(new SKRect(cx - rx, cy - ry, cx + rx, cy + ry));
            //ellipsePath.Transform(combinedTransform);
            generatedPath = new SKPath();
            generatedPath.AddPath(ellipsePath);
        }
        else if (element is SvgLine svgLine)
        {
            // Преобразуем SvgLine в SKPath
            var x1 = (float)svgLine.StartX;
            var y1 = (float)svgLine.StartY;
            var x2 = (float)svgLine.EndX;
            var y2 = (float)svgLine.EndY;

            using var linePath = new SKPath();
            linePath.MoveTo(x1, y1);
            linePath.LineTo(x2, y2);
            //linePath.Transform(combinedTransform);
            generatedPath = new SKPath();
            generatedPath.AddPath(linePath);
        }
        else if (element is SvgRectangle svgRectangle)
        {
            // Преобразуем SvgRectangle в SKPath
            var x = (float)svgRectangle.X;
            var y = (float)svgRectangle.Y;
            var width = (float)svgRectangle.Width;
            var height = (float)svgRectangle.Height;
            var rect = new SKRect(x, y, x + width, y + height);

            using var rectPath = new SKPath();
            rectPath.AddRect(rect);
            //rectPath.Transform(combinedTransform);
            generatedPath = new SKPath();
            generatedPath.AddPath(rectPath);
        }
        else if (element is SvgPolyline svgPolyline)
        {
            using var polylinePath = new SKPath();
            var points = svgPolyline.Points
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / 2) // Группируем по парам (X, Y)
                .Select(g => new SKPoint(
                    g.ElementAt(0).value.Value,
                    g.ElementAt(1).value.Value
                ))
                .ToArray();

            polylinePath.AddPoly(points, close: false);
            //polylinePath.Transform(combinedTransform);
            
            generatedPath = new SKPath();
            /*if (svgPolyline.FillRule == SvgFillRule.EvenOdd)
            {
                var path = SKPath.ParseSvgPathData(PathProcessor.GetFixedPathDataString(polylinePath.ToSvgPathData()));
                generatedPath.AddPath(path);
            }
            else*/
            {
                generatedPath.AddPath(polylinePath);
            }
        }
        else if (element is SvgPolygon svgPolygon)
        {
            using var polygonPath = new SKPath();
            var points = svgPolygon.Points
                .Select((value, index) => new { value, index })
                .GroupBy(x => x.index / 2) // Группируем по парам (X, Y)
                .Select(g => new SKPoint(
                    g.ElementAt(0).value.Value,
                    g.ElementAt(1).value.Value
                ))
                .ToArray();

            polygonPath.AddPoly(points, close: true);
            //polygonPath.Transform(combinedTransform);
            
            generatedPath = new SKPath();
            if (svgPolygon.FillRule == SvgFillRule.EvenOdd)
            {
                var path = SKPath.ParseSvgPathData(PathProcessor.GetFixedPathDataString(polygonPath.ToSvgPathData()));
                generatedPath.AddPath(path);
            }
            else
            {
                generatedPath.AddPath(polygonPath);;
            }
        }

        if (generatedPath != null)
        {
            generatedPath.Transform(combinedTransform);

            generatedPath.ApplyStroke(element);

            combinedPath.AddPath(generatedPath);
        }

        foreach (var child in element.Children)
        {
            TraverseSvgElement(child, combinedPath, combinedTransform);
        }
    }

    private static SKMatrix GetTransform(SvgElement element)
    {
        if (element.Transforms == null || element.Transforms.Count == 0)
        {
            return SKMatrix.Identity;
        }

        var skMatrix = SKMatrix.CreateIdentity();

        foreach (var transform in element.Transforms)
        {
            if (transform is Svg.Transforms.SvgTranslate translate)
            {
                skMatrix = skMatrix.PreConcat(SKMatrix.CreateTranslation(translate.X, translate.Y));
            }
            else if (transform is Svg.Transforms.SvgScale scale)
            {
                skMatrix = skMatrix.PreConcat(SKMatrix.CreateScale(scale.X, scale.Y));
            }
            else if (transform is Svg.Transforms.SvgRotate rotate)
            {
                // Вращение с указанием центра (если передано)
                float cx = rotate.CenterX;
                float cy = rotate.CenterY;

                skMatrix = skMatrix.PreConcat(
                    SKMatrix.CreateTranslation(-cx, -cy)
                        .PreConcat(SKMatrix.CreateRotationDegrees(rotate.Angle))
                        .PreConcat(SKMatrix.CreateTranslation(cx, cy))
                );
            }
            else if (transform is Svg.Transforms.SvgSkew skew)
            {
                // Скос по оси X
                if (skew.AngleX != 0)
                {
                    skMatrix = skMatrix.PreConcat(SKMatrix.CreateSkew((float)Math.Tan(skew.AngleX * Math.PI / 180), 0));
                }
                // Скос по оси Y
                else if (skew.AngleY != 0)
                {
                    skMatrix = skMatrix.PreConcat(SKMatrix.CreateSkew(0, (float)Math.Tan(skew.AngleY * Math.PI / 180)));
                }
            }
            else if (transform is Svg.Transforms.SvgMatrix svgMatrix)
            {
                var matrixValues = svgMatrix.Matrix;
                var skMatrixCustom = new SKMatrix
                {
                    ScaleX = matrixValues.Elements[0],
                    SkewX = matrixValues.Elements[2],
                    TransX = matrixValues.Elements[4],
                    SkewY = matrixValues.Elements[1],
                    ScaleY = matrixValues.Elements[3],
                    TransY = matrixValues.Elements[5],
                    Persp0 = 0,
                    Persp1 = 0,
                    Persp2 = 1
                };

                skMatrix = skMatrix.PreConcat(skMatrixCustom);
            }
        }

        return skMatrix;
    }

    private static void ApplyStroke(this SKPath path, SvgElement svgElement)
    {
        /*if (svgElement.Stroke == null || svgElement.Stroke == SvgPaintServer.None)
            return;*/

        SKPaintStyle style = SKPaintStyle.Fill;

        if ((svgElement.Stroke != null && svgElement.Stroke != SvgPaintServer.None) &&
            (svgElement.Fill == null || svgElement.Fill != SvgPaintServer.None))
        {
            style = SKPaintStyle.StrokeAndFill;
        }
        else if (svgElement.Stroke != null && svgElement.Stroke != SvgPaintServer.None)
        {
            style = SKPaintStyle.Stroke;
        }
        else if (svgElement.Fill == null || svgElement.Fill != SvgPaintServer.None)
        {
            style = SKPaintStyle.Fill;
        }

        var paint = new SKPaint
        {
            IsAntialias = true,
            Style = style,
            StrokeWidth = svgElement.StrokeWidth.Value,
            Color = GetSKColor(svgElement.Stroke).WithAlpha((byte)(svgElement.StrokeOpacity * 255)),
            StrokeCap = GetStrokeCap(svgElement.StrokeLineCap),
            StrokeJoin = GetStrokeJoin(svgElement.StrokeLineJoin),
            StrokeMiter = svgElement.StrokeMiterLimit
        };

        // Обработка пунктирных линий
        if (svgElement.StrokeDashArray != null && svgElement.StrokeDashArray.Any())
        {
            paint.PathEffect = SKPathEffect.CreateDash(
                svgElement.StrokeDashArray.Select(v => v.Value).ToArray(),
                svgElement.StrokeDashOffset.Value
            );
        }

        // Применяем обводку к пути
        paint.GetFillPath(path, path);
    }


    private static SKStrokeCap GetStrokeCap(SvgStrokeLineCap lineCap)
    {
        return lineCap switch
        {
            SvgStrokeLineCap.Round => SKStrokeCap.Round,
            SvgStrokeLineCap.Square => SKStrokeCap.Square,
            _ => SKStrokeCap.Butt // По умолчанию
        };
    }

    private static SKStrokeJoin GetStrokeJoin(SvgStrokeLineJoin lineJoin)
    {
        return lineJoin switch
        {
            SvgStrokeLineJoin.Round => SKStrokeJoin.Round,
            SvgStrokeLineJoin.Bevel => SKStrokeJoin.Bevel,
            _ => SKStrokeJoin.Miter // По умолчанию
        };
    }

    private static SKColor GetSKColor(SvgPaintServer? stroke)
    {
        if (stroke is SvgColourServer color)
        {
            return new SKColor(color.Colour.R, color.Colour.G, color.Colour.B, color.Colour.A);
        }

        return SKColors.Transparent; // Если цвет не задан
    }
}