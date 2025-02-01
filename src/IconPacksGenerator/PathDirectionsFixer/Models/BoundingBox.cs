using System.Drawing;

namespace IconPacksGenerator.PathDirectionsFixer.Models;

public class BoundingBox
{
    public float Left { get; set; }
    public float Right { get; set; }
    public float Top { get; set; }
    public float Bottom { get; set; }
    public float Width { get; set; }
    public float Height { get; set; }
    
    public static BoundingBox GetPolyBBox(List<PointF> vertices)
    {
        float[] xArr = vertices.Select(p => p.X).ToArray();
        float[] yArr = vertices.Select(p => p.Y).ToArray();

        return new BoundingBox
        {
            Left = xArr.Min(),
            Right = xArr.Max(),
            Top = yArr.Min(),
            Bottom = yArr.Max(),
            Width = xArr.Max() - xArr.Min(),
            Height = yArr.Max() - yArr.Min()
        };
    }
}