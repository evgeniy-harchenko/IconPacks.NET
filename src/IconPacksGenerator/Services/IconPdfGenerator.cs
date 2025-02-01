using SkiaSharp;

namespace IconPacksGenerator.Services;

internal static class IconPdfGenerator
{
    public static void GeneratePdf(string iconsClassName, Dictionary<string, string> iconsByClass)
    {
        const int PageWidth = 595; // A4 Width in points
        const int PageHeight = 842; // A4 Height in points
        const int Margin = 40;
        const int IconSize = 20;
        const int RowHeight = 40;
        const int ColumnGap = 20;
        const int IconColumnWidth = IconSize + ColumnGap;

        using var document = SKDocument.CreatePdf(iconsClassName + ".pdf");
        var paint = new SKPaint
        {
            Typeface = SKTypeface.FromFamilyName("Arial"),
            TextSize = 20,
            IsAntialias = true
        };

        var headerPaint = new SKPaint
        {
            Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Bold),
            TextSize = 30,
            IsAntialias = true
        };

        float x = Margin;
        float y = Margin;

        SKCanvas? canvas = null;

        try
        {
            canvas = document.BeginPage(PageWidth, PageHeight);

            canvas.DrawText($"{iconsClassName}", x, y, headerPaint);
            y += RowHeight;

            foreach (var (iconName, svgPath) in iconsByClass)
            {
                // Проверяем, нужно ли начинать новую страницу
                if (y + RowHeight > PageHeight - Margin)
                {
                    document.EndPage();
                    canvas = document.BeginPage(PageWidth, PageHeight);
                    y = Margin; // Сбрасываем позицию по вертикали
                }

                // Рисуем иконку
                using var path = SKPath.ParseSvgPathData(svgPath);
                if (path == null)
                {
                    Console.WriteLine($"{iconsClassName}: Failed to generate pdf for {iconName}: {svgPath}");
                    continue;
                }

                var scale = IconSize / Math.Max(path.Bounds.Width, path.Bounds.Height);
                path.Transform(SKMatrix.CreateScale(scale, scale));
                var iconX = x + (IconSize - path.Bounds.Width * scale) / 2;
                var iconY = y + (RowHeight - path.Bounds.Height * scale) / 2;

                canvas.Save();
                canvas.Translate(iconX, iconY);
                canvas.DrawPath(path, paint);
                canvas.Restore();

                // Рисуем текст
                canvas.DrawText(iconName, x + IconColumnWidth, y + RowHeight / 2 + paint.TextSize / 2, paint);

                // Сдвигаем позицию вниз для следующего элемента
                y += RowHeight;
            }

            document.EndPage();
        }
        finally
        {
            canvas?.Dispose();
        }
    }
}