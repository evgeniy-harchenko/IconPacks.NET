namespace IconPacksGenerator.PathDirectionsFixer.Models;

public class Options
{
    public bool ArcToCubic { get; init; } = false;
    public bool QuadraticToCubic { get; init; } = false;
    public bool ToClockwise { get; init; } = false;
    public bool ReturnD { get; init; } = false;
    public int Decimals { get; init; } = -1;
    public bool ToAbsolute { get; init; } = true;
    public bool ToLonghands { get; init; } = true;
    public int ArcAccuracy { get; init; } = 1;

    public Options MergeWith(Options? newOptions = null)
    {
        if (newOptions == null)
            return this;

        return new Options
        {
            ArcToCubic = newOptions.ArcToCubic,
            QuadraticToCubic = newOptions.QuadraticToCubic,
            ToClockwise = newOptions.ToClockwise,
            ReturnD = newOptions.ReturnD,
            Decimals = newOptions.Decimals,
            ToAbsolute = newOptions.ToAbsolute,
            ToLonghands = newOptions.ToLonghands,
            ArcAccuracy = newOptions.ArcAccuracy
        };
    }

    public override string ToString()
    {
        return $"ArcToCubic: {ArcToCubic}, QuadraticToCubic: {QuadraticToCubic}, " +
               $"ToClockwise: {ToClockwise}, ReturnD: {ReturnD}, " +
               $"Decimals: {Decimals}, ToAbsolute: {ToAbsolute}, " +
               $"ToLonghands: {ToLonghands}, ArcAccuracy: {ArcAccuracy}";
    }
}