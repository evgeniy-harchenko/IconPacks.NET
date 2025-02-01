using System.Drawing;
using IconPacksGenerator.PathDirectionsFixer.Models;

namespace IconPacksGenerator.PathDirectionsFixer.Utilities;

public static class PolygonUtilities
{
    public static bool IsPointInPolygon(PointF pt, List<PointF> polygon, BoundingBox bb, bool skipBB = false)
    {
        bool Between(float value, float a, float b) => (value >= a && value <= b) || (value <= a && value >= b);

        if (!skipBB && (pt.X < bb.Left || pt.X > bb.Right || pt.Y < bb.Top || pt.Y > bb.Bottom))
            return false;

        bool inside = false;
        int n = polygon.Count;
        for (int i = n - 1, j = 0; j < n; i = j, j++)
        {
            PointF a = polygon[i];
            PointF b = polygon[j];

            // Точное сравнение координат как в JS
            if ((pt.X == a.X && pt.Y == a.Y) || (pt.X == b.X && pt.Y == b.Y))
                return true;

            if (a.Y == b.Y && pt.Y == a.Y && Between(pt.X, a.X, b.X))
                return true;

            if (Between(pt.Y, a.Y, b.Y))
            {
                if ((pt.Y == a.Y && b.Y >= a.Y) || (pt.Y == b.Y && a.Y >= b.Y))
                    continue;

                float cross = (a.X - pt.X) * (b.Y - pt.Y) - (b.X - pt.X) * (a.Y - pt.Y);
                if (cross == 0) return true;
                if (a.Y < b.Y == cross > 0) inside = !inside;
            }
        }

        return inside;
    }

    public static float PolygonArea(List<PointF> points, bool absolute = false)
    {
        float area = 0.0f;
        for (int i = 0; i < points.Count; i++)
        {
            float addX = points[i].X;
            float addY = points[i == points.Count - 1 ? 0 : i + 1].Y;
            float subX = points[i == points.Count - 1 ? 0 : i + 1].X;
            float subY = points[i].Y;
            area += addX * addY * 0.5f - subX * subY * 0.5f;
        }

        if (absolute)
        {
            area = Math.Abs(area);
        }

        return area;
    }

    public static List<PointF> GetPathDataPoly(List<PathCommand> pathData)
    {
        List<PointF> poly = new List<PointF>();

        for (int i = 0; i < pathData.Count; i++)
        {
            var com = pathData[i];
            var prev = i > 0 ? pathData[i - 1] : pathData[i];
            var type = com.Type;
            var values = com.Values;
            PointF p0 = new PointF { X = prev.Values[prev.Values.Count - 2], Y = prev.Values[prev.Values.Count - 1] };
            PointF? p = values.Count > 0
                ? new PointF(
                    values[values.Count - 2],
                    values[values.Count - 1]
                )
                : null;

            PointF? cp1 = values.Count > 0 ? new PointF(values[0], values[1]) : null;

            switch (type)
            {
                // convert to cubic to get polygon
                case "A":
                    List<PathCommand> cubic = CurveUtilities.ArcToBezier(p0, values);
                    foreach (PathCommand cmd in cubic)
                    {
                        var vals = cmd.Values;
                        poly.Add(new PointF(vals[0], vals[1]));
                        poly.Add(new PointF(vals[2], vals[3]));
                        poly.Add(new PointF(vals[4], vals[5]));
                    }

                    break;

                case "C":
                    if (cp1.HasValue)
                    {
                        poly.Add(cp1.Value);
                        poly.Add(new PointF(values[2], values[3]));
                    }

                    break;
                case "Q":
                    if (cp1.HasValue)
                        poly.Add(cp1.Value);
                    break;
            }

            // M and L commands
            if (type.ToLower() != "z")
            {
                if (p.HasValue)
                    poly.Add(p.Value);
            }
        }

        return poly;
    }
}