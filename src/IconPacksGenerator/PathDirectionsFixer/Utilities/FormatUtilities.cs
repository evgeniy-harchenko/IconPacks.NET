using System.Globalization;
using System.Text;
using IconPacksGenerator.PathDirectionsFixer.Models;

namespace IconPacksGenerator.PathDirectionsFixer.Utilities;

public static class FormatUtilities
{
    public static string PathDataToD(List<PathCommand> pathData, int decimals = -1, bool minify = false)
    {
        // implicit l command
        if (pathData[1].Type == "l" && minify)
        {
            pathData[0].Type = "m";
        }

        var d = new StringBuilder();
        d.Append($"{pathData[0].Type}{FormatValues(pathData[0].Values, decimals, minify)}");

        for (int i = 1; i < pathData.Count; i++)
        {
            var com0 = pathData[i - 1];
            var com = pathData[i];
            var type = com.Type;
            var values = com.Values;

            // minify arctos
            if (minify && type == "A" || type == "a")
            {
                values = new List<float>
                {
                    values[0],
                    values[1],
                    values[2],
                    float.Parse($"{values[3]}{values[4]}{values[5]}"),
                    values[6]
                };
            }

            // omit type for repeated commands
            type = (com0.Type == com.Type && com.Type.ToLower() != "m" && minify) ? " " :
            (
                (com0.Type == "m" && com.Type == "l") ||
                (com0.Type == "M" && com.Type == "l") ||
                (com0.Type == "M" && com.Type == "L")
            ) && minify ? " " : com.Type;

            d.Append($"{type}{FormatValues(values, decimals, minify)}");
        }

        if (minify)
        {
            d = d
                .Replace(" 0.", " .")
                .Replace(" -", "-")
                .Replace("-0.", "-.")
                .Replace("Z", "z");
        }

        return d.ToString();
    }

    public static string FormatValues(List<float> values, int decimals, bool minify)
    {
        var culture = CultureInfo.InvariantCulture;
        var formatted = new StringBuilder();

        foreach (var val in values)
        {
            string numStr = decimals > -1
                ? Math.Round(val, decimals).ToString("0.################", culture)
                : val.ToString("0.################", culture);

            if (minify)
            {
                numStr = numStr.Replace(".0", "") // 2.0 → 2
                    .Replace("0.", "."); // 0.5 → .5
            }

            formatted.Append($" {numStr}");
        }

        return formatted.ToString().TrimStart();
    }
}