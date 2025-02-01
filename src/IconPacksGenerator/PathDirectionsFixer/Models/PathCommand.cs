namespace IconPacksGenerator.PathDirectionsFixer.Models;

public class PathCommand
{
    public string Type { get; set; } = string.Empty;
    public List<float> Values { get; set; } = new List<float>();
}