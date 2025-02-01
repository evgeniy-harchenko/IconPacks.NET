using System.Drawing;
using IconPacksGenerator.PathDirectionsFixer.Models;
using IconPacksGenerator.PathDirectionsFixer.Utilities;

namespace IconPacksGenerator.PathDirectionsFixer.Core;

public static class PathProcessor
{
    // Основные публичные методы
    public static List<PathCommand> GetFixedPathData(string d, Options? options = null)
    {
        options ??= new Options();

        var toClockwise = options.ToClockwise;
        var pathData = PathParser.ParsePathDataNormalized(d, options);
        var pathDataFixed = FixPathDataDirections(pathData, toClockwise);

        return pathDataFixed;
    }

    public static string GetFixedPathDataString(string d, Options? options = null)
    {
        options ??= new Options();

        return FormatUtilities.PathDataToD(GetFixedPathData(d, options), options.Decimals);
    }

    // Исправление направлений
    public static List<PathCommand> FixPathDataDirections(List<PathCommand> pathData, bool toClockwise = false,
        bool sort = true)
    {
        // 5. Основная логика функции
        List<PathCommand> clonedData = pathData.Select(p => new PathCommand
        {
            Type = p.Type,
            Values = new List<float>(p.Values)
        }).ToList();

        List<List<PathCommand>> pathDataArr = PathUtilities.SplitSubpaths(clonedData);
        List<PolyInfo> polys = new List<PolyInfo>();

        // 6. Анализ подпутей
        for (int i = 0; i < pathDataArr.Count; i++)
        {
            List<PointF> vertices = PolygonUtilities.GetPathDataPoly(pathDataArr[i]);
            float area = PolygonUtilities.PolygonArea(vertices);
            bool isClockwise = area >= 0;

            polys.Add(new PolyInfo
            {
                Points = vertices,
                BBox = BoundingBox.GetPolyBBox(vertices),
                IsClockwise = isClockwise,
                Index = i,
                Inter = 0,
                Includes = new List<int>(),
                IncludedIn = new List<int>()
            });
        }

        // 7. Проверка пересечений
        for (int i = 0; i < polys.Count; i++)
        {
            PolyInfo prev = polys[i];
            for (int j = 0; j < polys.Count; j++)
            {
                if (i == j || polys[j].Includes.Contains(i)) continue;

                PolyInfo poly = polys[j];
                PointF ptMid = new PointF(
                    poly.BBox.Left + poly.BBox.Width / 2,
                    poly.BBox.Top + poly.BBox.Height / 2
                );

                if (PolygonUtilities.IsPointInPolygon(ptMid, prev.Points, prev.BBox))
                {
                    polys[j].Inter++;
                    polys[j].IncludedIn.Add(i);
                    prev.Includes.Add(j);
                }
            }
        }

        // 8. Реверс путей
        for (int i = 0; i < polys.Count; i++)
        {
            var poly = polys[i];
            var cw = poly.IsClockwise;
            var includedIn = poly.IncludedIn;
            var includes = poly.Includes;

            // outer path direction to counter clockwise
            if (!(includedIn.Count > 0) && cw && !toClockwise
                || !(includedIn.Count > 0) && !cw && toClockwise
               )
            {
                pathDataArr[i] = PathUtilities.ReversePathData(pathDataArr[i]);
                polys[i].IsClockwise = !polys[i].IsClockwise;
                cw = polys[i].IsClockwise;
            }

            // reverse inner sub paths
            for (int j = 0; j < includes.Count; j++)
            {
                var ind = includes[j];
                var child = polys[ind];

                if (child.IsClockwise == cw)
                {
                    pathDataArr[ind] = PathUtilities.ReversePathData(pathDataArr[ind]);
                    polys[ind].IsClockwise = polys[ind].IsClockwise ? false : true;
                }
            }
        }

        if (sort)
        {
            polys.Sort((a, b) =>
                a.BBox.Top.CompareTo(b.BBox.Top) != 0 ? a.BBox.Top.CompareTo(b.BBox.Top) :
                a.BBox.Width.CompareTo(b.BBox.Width) != 0 ? a.BBox.Width.CompareTo(b.BBox.Width) :
                a.BBox.Left.CompareTo(b.BBox.Left));

            pathDataArr = polys.Select(poly => pathDataArr[poly.Index]).ToList();
        }

        return pathDataArr.SelectMany(x => x).ToList();
    }
}