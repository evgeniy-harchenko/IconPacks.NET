using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;
using IconPacksGenerator.PathDirectionsFixer.Models;
using IconPacksGenerator.PathDirectionsFixer.Utilities;
using SkiaSharp;

namespace IconPacksGenerator.PathDirectionsFixer.Core;

public class PathParser
{
    public static List<PathCommand> ParsePathDataNormalized(string d, Options? options = null)
    {
        List<PathCommand> pathData = new List<PathCommand>();
        Dictionary<string, int> comLengths = new Dictionary<string, int>
        {
            { "m", 2 }, { "a", 7 }, { "c", 6 }, { "h", 1 },
            { "l", 2 }, { "q", 4 }, { "s", 4 }, { "t", 2 },
            { "v", 1 }, { "z", 0 }
        };

        // Normalize input string
        d = Regex.Replace(d, @"[\n\r\t|,]", " ")
            .Trim()
            .Replace(@"(\d)-", "$1 -")
            .Replace(@"(\.)(?=(\d+\.\d+)+)(\d+)", "$1$3 ");

        var cmdRegEx = new Regex(@"([mlcqazvhst])([^mlcqazvhst]*)", RegexOptions.IgnoreCase);
        var matches = cmdRegEx.Matches(d);

        Options mergedOptions = new Options
        {
            ToAbsolute = true,
            ToLonghands = true,
            ArcToCubic = false,
            QuadraticToCubic = false,
            ArcAccuracy = 1
        }.MergeWith(options);

        var toAbsolute = mergedOptions.ToAbsolute;
        var toLonghands = mergedOptions.ToLonghands;
        var arcToCubic = mergedOptions.ArcToCubic;
        var arcAccuracy = mergedOptions.ArcAccuracy;
        var quadraticToCubic = mergedOptions.QuadraticToCubic;

        var hasArcs = Regex.IsMatch(d, "[a]", RegexOptions.IgnoreCase);
        var hasShorthands = toLonghands && Regex.IsMatch(d, "[vhst]", RegexOptions.IgnoreCase);
        var hasRelative = toAbsolute && Regex.IsMatch(d.Substring(1, d.Length - 2), "[lcqamts]");
        var hasQuadratics = quadraticToCubic && Regex.IsMatch(d, "[qt]", RegexOptions.IgnoreCase);

        float offX = 0, offY = 0, lastX = 0, lastY = 0;
        SKPoint m = SKPoint.Empty;

        for (int c = 0; c < matches.Count; c++)
        {
            var com = matches[c];
            var type = com.Groups[1].Value;
            var valuesStr = com.Groups[2].Value.Trim();
            var typeRel = type.ToLower();
            var typeAbs = type.ToUpper();
            var chunkSize = comLengths[typeRel];
            var isRel = type == typeRel;

            // Parse values
            var values = Regex.Matches(valuesStr, @"-?\d*\.?\d+(?:e[+-]?\d+)?")
                .Select(match => float.Parse(match.Value, CultureInfo.InvariantCulture))
                .ToList();

            // Handle Arc command parameters
            if (typeRel == "a" && values.Count != comLengths["a"])
            {
                var arcValues = new List<string>();
                int n = 0;
                foreach (var value in valuesStr.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (n >= 7) n = 0;
                    if ((n == 3 || n == 4) && value.Length > 1)
                    {
                        arcValues.Add(value.Substring(0, 1));
                        arcValues.Add(value.Substring(1, 1));
                        arcValues.Add(value.Substring(2));
                        n += 3;
                    }
                    else
                    {
                        arcValues.Add(value);
                        n++;
                    }
                }

                values = arcValues.Select(float.Parse).ToList();
            }

            // Split into chunks
            var comChunks = new List<PathCommand>();
            comChunks.Add(new PathCommand { Type = type, Values = values });
            if (values.Count > chunkSize)
            {
                var typeImplicit = typeRel == "m" ? (isRel ? "l" : "L") : type;
                for (int i = chunkSize; i < values.Count; i += chunkSize)
                {
                    comChunks.Add(new PathCommand
                    {
                        Type = i == 0 ? type : typeImplicit,
                        Values = values.Skip(i).Take(chunkSize).ToList()
                    });
                }
            }

            if (!hasRelative && !hasShorthands && !hasArcs && !hasQuadratics)
            {
                comChunks.ForEach(com1 => { pathData.Add(com1); });
            }
            else
            {
                if (c == 0)
                {
                    offX = values[0];
                    offY = values[1];
                    lastX = offX;
                    lastY = offY;
                    m = new SKPoint(offX, offY);
                }

                var typeFirst = comChunks[0].Type;
                typeAbs = typeFirst.ToUpper();
                isRel = typeFirst.ToLower() == typeFirst && pathData.Count > 0;

                for (int i = 0; i < comChunks.Count; i++)
                {
                    var comChunk = comChunks[i];
                    string chunkType = comChunk.Type;
                    List<float> valuesAbs = comChunk.Values.ToList();
                    int valuesAbsL = valuesAbs.Count;
                    PathCommand comPrev;

                    if (i - 1 >= 0 && comChunks[i - 1] != null)
                    {
                        comPrev = comChunks[i - 1];
                    }
                    else if (c > 0 && pathData.Count > 0 && pathData[pathData.Count - 1] != null)
                    {
                        comPrev = pathData[pathData.Count - 1];
                    }
                    else
                    {
                        comPrev = comChunks[i];
                    }

                    var valuesPrev = comPrev.Values;
                    var valuesPrevL = valuesPrev.Count;
                    isRel = comChunks.Count > 1 ? chunkType.ToLower() == chunkType && pathData.Count > 0 : isRel;

                    if (isRel)
                    {
                        comChunk.Type = comChunks.Count > 1 ? chunkType.ToUpper() : typeAbs;
                        comChunk.Values.Clear();
                        switch (typeRel)
                        {
                            case "a":
                                comChunk.Values.AddRange(valuesAbs.Take(5));
                                comChunk.Values.Add(valuesAbs[5] + offX);
                                comChunk.Values.Add(valuesAbs[6] + offY);
                                break;
                            case "h":
                                comChunk.Values.Add(valuesAbs[0] + offX);
                                break;
                            case "v":
                                comChunk.Values.Add(valuesAbs[0] + offY);
                                break;
                            case "m":
                            case "l":
                            case "t":
                                comChunk.Values.Add(valuesAbs[0] + offX);
                                comChunk.Values.Add(valuesAbs[1] + offY);
                                if (chunkType == "m")
                                    m = new SKPoint(valuesAbs[0] + offX, valuesAbs[1] + offY);
                                break;
                            case "c":
                                comChunk.Values.AddRange(new[]
                                {
                                    valuesAbs[0] + offX, valuesAbs[1] + offY,
                                    valuesAbs[2] + offX, valuesAbs[3] + offY,
                                    valuesAbs[4] + offX, valuesAbs[5] + offY
                                });
                                break;
                            case "q":
                            case "s":
                                comChunk.Values.AddRange(new[]
                                {
                                    valuesAbs[0] + offX, valuesAbs[1] + offY,
                                    valuesAbs[2] + offX, valuesAbs[3] + offY
                                });
                                break;
                            case "z":
                            case "Z":
                                lastX = m.X;
                                lastY = m.Y;
                                break;
                        }
                    }
                    // is absolute
                    else
                    {
                        offX = 0;
                        offY = 0;
                    }

                    // Handle shorthands
                    if (hasShorthands)
                    {
                        if (comChunk.Type == "H" || comChunk.Type == "V")
                        {
                            comChunk.Values =
                                comChunk.Type == "H"
                                    ? [comChunk.Values[0], lastY]
                                    : [lastX, comChunk.Values[0]];
                            comChunk.Type = "L";
                        }
                        else if (comChunk.Type == "T" || comChunk.Type == "S")
                        {
                            var cp1X = valuesPrev[0];
                            var cp1Y = valuesPrev[1];

                            var cp2X = valuesPrevL > 2 ? valuesPrev[2] : valuesPrev[0];
                            var cp2Y = valuesPrevL > 2 ? valuesPrev[3] : valuesPrev[1];

                            // new control point
                            var cpN1X = comChunk.Type == "T" ? lastX * 2 - cp1X : lastX * 2 - cp2X;
                            var cpN1Y = comChunk.Type == "T" ? lastY * 2 - cp1Y : lastY * 2 - cp2Y;
                            comChunk.Values = new[] { cpN1X, cpN1Y }.Concat(comChunk.Values).ToList();
                            comChunk.Type = comChunk.Type == "T" ? "Q" : "C";
                        }
                    }

                    PointF p0 = new PointF() { X = lastX, Y = lastY };
                    if (arcToCubic && hasArcs && comChunk.Type == "A")
                    {
                        if (typeRel == "a")
                        {
                            var comArc = CurveUtilities.ArcToBezier(p0, comChunk.Values, arcAccuracy);
                            comArc.ForEach(seg => { pathData.Add(seg); });
                        }
                    }

                    else
                    {
                        pathData.Add(comChunk);
                    }

                    lastX =
                        valuesAbsL > 1
                            ? valuesAbs[valuesAbsL - 2] + offX
                            : typeRel == "h"
                                ? valuesAbs[0] + offX
                                : lastX;
                    lastY =
                        valuesAbsL > 1
                            ? valuesAbs[valuesAbsL - 1] + offY
                            : typeRel == "v"
                                ? valuesAbs[0] + offY
                                : lastY;
                    offX = lastX;
                    offY = lastY;
                }
            }
        }

        if (toAbsolute)
        {
            pathData[0].Type = "M";
        }
        //pathData[0].Type = "M";

        // Final conversions
        for (int i = 0; i < pathData.Count; i++)
        {
            var com = pathData[i];
            if (quadraticToCubic && hasQuadratics && com.Type == "Q")
            {
                var comPrev = pathData[i - 1];
                var comPrevValues = comPrev.Values;
                var comPrevValuesL = comPrevValues.Count;
                PointF p0 = new PointF()
                {
                    X = comPrevValues[comPrevValuesL - 2],
                    Y = comPrevValues[comPrevValuesL - 1]
                };
                pathData[i] = CurveUtilities.Quadratic2Cubic(p0, com.Values);
            }

            com.Values = com.Values.Select(v =>
                    (float)Math.Round(v, 9, MidpointRounding.AwayFromZero) // Корректное округление
            ).ToList();

            pathData[i].Values = pathData[i].Values.Count > 1
                ? pathData[i].Values.Select(v =>
                    (float)Math.Round(v, 9, MidpointRounding.AwayFromZero)).ToList()
                : com.Values;
        }

        return pathData;
    }
}