using System.Drawing;
using IconPacksGenerator.PathDirectionsFixer.Models;

namespace IconPacksGenerator.PathDirectionsFixer.Utilities;

public static class CurveUtilities
{
    public static PathCommand Quadratic2Cubic(PointF p0, List<float> values)
    {
        PointF cp1 = new PointF()
        {
            X = p0.X + 2f / 3f * (values[0] - p0.X),
            Y = p0.Y + 2f / 3f * (values[1] - p0.Y)
        };
        PointF cp2 = new PointF()
        {
            X = values[2] + 2f / 3f * (values[0] - values[2]),
            Y = values[3] + 2f / 3f * (values[1] - values[3])
        };

        return new PathCommand()
        {
            Type = "C",
            Values = { cp1.X, cp1.Y, cp2.X, cp2.Y, values[2], values[3] }
        };
    }

    public static List<PathCommand> ArcToBezier(PointF p0, List<float> values, int splitSegments = 1)
    {
        float rx = values[0];
        float ry = values[1];
        float rotation = values[2];
        float largeArcFlag = values[3];
        float sweepFlag = values[4];
        float x = values[5];
        float y = values[6];

        if (rx == 0 || ry == 0)
        {
            return new List<PathCommand>();
        }

        float phi = rotation != 0 ? rotation * MathF.Tau / 360 : 0;
        float sinphi = phi != 0 ? MathF.Sin(phi) : 0;
        float cosphi = phi != 0 ? MathF.Cos(phi) : 1;
        float pxp = cosphi * (p0.X - x) / 2 + sinphi * (p0.Y - y) / 2;
        float pyp = -sinphi * (p0.X - x) / 2 + cosphi * (p0.Y - y) / 2;

        if (pxp == 0 && pyp == 0)
        {
            return new List<PathCommand>();
        }

        rx = Math.Abs(rx);
        ry = Math.Abs(ry);
        float lambda =
            pxp * pxp / (rx * rx) +
            pyp * pyp / (ry * ry);
        if (lambda > 1)
        {
            float lambdaRt = MathF.Sqrt(lambda);
            rx *= lambdaRt;
            ry *= lambdaRt;
        }

        float rxsq = rx * rx;
        float rysq = rx == ry ? rxsq : ry * ry;

        float pxpsq = pxp * pxp;
        float pypsq = pyp * pyp;
        float radicant = (rxsq * rysq) - (rxsq * pypsq) - (rysq * pxpsq);

        if (radicant <= 0)
        {
            radicant = 0;
        }
        else
        {
            radicant /= (rxsq * pypsq) + (rysq * pxpsq);
            radicant = MathF.Sqrt(radicant) * (largeArcFlag == sweepFlag ? -1 : 1);
        }

        float centerxp = radicant != 0 ? radicant * rx / ry * pyp : 0;
        float centeryp = radicant != 0 ? radicant * -ry / rx * pxp : 0;
        float centerx = cosphi * centerxp - sinphi * centeryp + (p0.X + x) / 2;
        float centery = sinphi * centerxp + cosphi * centeryp + (p0.Y + y) / 2;

        float vx1 = (pxp - centerxp) / rx;
        float vy1 = (pyp - centeryp) / ry;
        float vx2 = (-pxp - centerxp) / rx;
        float vy2 = (-pyp - centeryp) / ry;

        // get start and end angle
        static float VectorAngle(float ux, float uy, float vx, float vy)
        {
            float dot = ux * vx + uy * vy;

            // Ограничиваем диапазон от -1 до 1
            dot = MathF.Max(-1f, MathF.Min(1f, dot));

            // Проверка на коллинеарность
            if (dot == 1f) return 0f;
            if (dot == -1f) return MathF.PI;

            // Определяем знак угла
            float sign = (ux * vy - uy * vx < 0) ? -1f : 1f;

            // Возвращаем угол в радианах
            return sign * MathF.Acos(dot);
        }

        float ang1 = VectorAngle(1, 0, vx1, vy1);
        float ang2 = VectorAngle(vx1, vy1, vx2, vy2);

        if (sweepFlag == 0 && ang2 > 0)
        {
            ang2 -= MathF.PI * 2;
        }
        else if (sweepFlag == 1 && ang2 < 0)
        {
            ang2 += MathF.PI * 2;
        }

        float ratio = MathF.Round(MathF.Abs(ang2) / (MathF.Tau / 4));
        ratio = ratio == 0 ? 1 : ratio;

        // increase segments for more accureate length calculations
        float segments = ratio * splitSegments;
        ang2 /= segments;
        List<PathCommand> pathDataArc = new List<PathCommand>();


        // If 90 degree circular arc, use a constant
        // https://pomax.github.io/bezierinfo/#circles_cubic
        // k=0.551784777779014
        const float angle90 = 1.5707963267948966f;
        const float k = 0.551785f;
        float a = ang2 == angle90 ? k : (ang2 == -angle90 ? -k : 4f / 3f * MathF.Tan(ang2 / 4));

        float cos2 = ang2 != 0 ? MathF.Cos(ang2) : 1;
        float sin2 = ang2 != 0 ? MathF.Sin(ang2) : 0;
        string type = "C";

        static List<PointF> ApproxUnitArc(float ang1, float ang2, float a, float cos2, float sin2)
        {
            float x1 = ang1 != ang2 ? MathF.Cos(ang1) : cos2;
            float y1 = ang1 != ang2 ? MathF.Sin(ang1) : sin2;
            float angleSum = ang1 + ang2;
            float x2 = MathF.Cos(angleSum);
            float y2 = MathF.Sin(angleSum);

            return new List<PointF>()
            {
                new PointF(x1 - y1 * a, y1 + x1 * a),
                new PointF(x2 + y2 * a, y2 - x2 * a),
                new PointF(x2, y2)
            };
        }

        for (int i = 0; i < segments; i++)
        {
            var com = new PathCommand { Type = type, Values = new List<float>() };
            var curve = ApproxUnitArc(ang1, ang2, a, cos2, sin2);

            curve.ForEach((pt) =>
            {
                var x = pt.X * rx;
                var y = pt.Y * ry;
                com.Values.Add(cosphi * x - sinphi * y + centerx);
                com.Values.Add(sinphi * x + cosphi * y + centery);
            });
            pathDataArc.Add(com);
            ang1 += ang2;
        }

        return pathDataArc;
    }
}