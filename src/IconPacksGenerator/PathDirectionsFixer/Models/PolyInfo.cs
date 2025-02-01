using System.Drawing;

namespace IconPacksGenerator.PathDirectionsFixer.Models;

public class PolyInfo
{
    public List<PointF> Points { get; set; } = new List<PointF>();
    public BoundingBox BBox { get; set; } = new BoundingBox();
    public bool IsClockwise { get; set; }
    public int Index { get; set; }
    public int Inter { get; set; }
    public List<int> Includes { get; set; } = new List<int>();
    public List<int> IncludedIn { get; set; } = new List<int>();
}