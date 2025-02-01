using IconPacksGenerator.PathDirectionsFixer.Core;
using IconPacksGenerator.PathDirectionsFixer.Models;

namespace IconPacksGenerator.PathDirectionsFixer.Utilities;

public static class PathUtilities
{
    public static List<PathCommand> ReversePathData(object pathDataInput,
        Options? options = null)
    {
        // Инициализация опций
        Options mergedOptions = new Options
        {
            ArcToCubic = false,
            QuadraticToCubic = false,
            ToClockwise = false,
            ReturnD = false
        }.MergeWith(options);

        // Нормализация входных данных
        var pathDataNormalized = pathDataInput is List<PathCommand> pathCommands
            ? new List<PathCommand>(pathCommands)
            : pathDataInput is string pathD
                ? PathParser.ParsePathDataNormalized(pathD, mergedOptions)
                : throw new ArgumentException("Invalid input type. Expected " + nameof(List<PathCommand>) +
                                              " or a string containing SVG path data.");

        // Разделение на подпути
        var pathDataArr = pathDataInput is List<PathCommand>
            ? new List<List<PathCommand>> { pathDataNormalized }
            : SplitSubpaths(pathDataNormalized);

        var pathDataNew = new List<PathCommand>();

        for (int j = 0; j < pathDataArr.Count; j++)
        {
            var pathData = pathDataArr[j];
            bool closed =
                pathData[pathData.Count - 1].Type.ToLower() == "z";
            if (closed)
            {
                // add lineto closing space between Z and M
                pathData = AddClosePathLineto(pathData);
                // remove Z closepath
                pathData.Remove(pathData.Last());
            }

            // define last point as new M if path isn't closed
            var valuesLast = pathData[pathData.Count - 1].Values;
            var valuesLastL = valuesLast.Count;
            var m = closed
                ? pathData[0]
                : new PathCommand()
                {
                    Type = "M",
                    Values = [valuesLast[valuesLastL - 2], valuesLast[valuesLastL - 1]]
                };
            // starting M stays the same – unless the path is not closed
            pathDataNew.Add(m);

            // reverse path data command order for processing
            pathData.Reverse();
            for (int i = 1; i < pathData.Count; i++)
            {
                var com = pathData[i];
                var type = com.Type;
                var values = com.Values;
                var comPrev = pathData[i - 1];
                var typePrev = comPrev.Type;
                var valuesPrev = comPrev.Values;

                // get reversed control points and new end coordinates
                var controlPointsPrev = ReverseControlPoints(typePrev, valuesPrev).controlPoints;
                var endPoints = ReverseControlPoints(type, values).endPoints;

                // create new path data
                var newValues = controlPointsPrev.Concat(endPoints).SelectMany(list => list).ToList();
                pathDataNew.Add(new PathCommand()
                {
                    Type = typePrev,
                    Values = newValues
                });
            }

            // add previously removed Z close path
            if (closed)
            {
                pathDataNew.Add(new PathCommand()
                {
                    Type = "z",
                    Values = []
                });
            }
        }

        return pathDataNew;
    }
    
    public static List<List<PathCommand>> SplitSubpaths(List<PathCommand> pathData)
    {
        var subPathArr = new List<List<PathCommand>>();

        // Split segments after M command
        var subPathIndices = new List<int>();
        for (int i = 0; i < pathData.Count; i++)
        {
            if (pathData[i].Type == "M")
            {
                subPathIndices.Add(i);
            }
        }

        // No compound path
        if (subPathIndices.Count == 1)
        {
            return new List<List<PathCommand>> { pathData };
        }

        for (int i = 0; i < subPathIndices.Count; i++)
        {
            int startIndex = subPathIndices[i];
            int endIndex = (i + 1 < subPathIndices.Count) ? subPathIndices[i + 1] : pathData.Count;

            // Добавляем подсписок в результат
            subPathArr.Add(pathData.GetRange(startIndex, endIndex - startIndex));
        }

        return subPathArr;
    }
    
    // Вспомогательная функция для добавления закрывающей линии
    private static List<PathCommand> AddClosePathLineto(List<PathCommand> pathData)
    {
        bool closed = pathData.Last().Type.Equals("z", StringComparison.OrdinalIgnoreCase);
        var m = pathData.First();
        var x0 = m.Values[0];
        var y0 = m.Values[1];

        var lastCom = closed ? pathData[pathData.Count - 2] : pathData.Last();
        var xE = lastCom.Values[lastCom.Values.Count - 2];
        var yE = lastCom.Values[lastCom.Values.Count - 1];

        if (closed && (Math.Abs(x0 - xE) > float.Epsilon || Math.Abs(y0 - yE) > float.Epsilon))
        {
            pathData.RemoveAt(pathData.Count - 1);
            pathData.Add(new PathCommand { Type = "L", Values = new List<float> { x0, y0 } });
            pathData.Add(new PathCommand { Type = "Z", Values = new List<float>() });
        }

        return pathData;
    }

    private static (List<List<float>> controlPoints, List<List<float>> endPoints)
        ReverseControlPoints(string type, List<float> values)
    {
        var controlPoints = new List<List<float>>();
        var endPoints = new List<List<float>>();

        if (!type.Equals("A", StringComparison.OrdinalIgnoreCase))
        {
            // Обработка кривых Безье
            for (int i = 0; i < values.Count; i += 2)
            {
                controlPoints.Add(new List<float> { values[i], values[i + 1] });
            }

            endPoints.Add(controlPoints.Last());
            controlPoints.RemoveAt(controlPoints.Count - 1);
            controlPoints.Reverse();
        }
        else
        {
            // Обработка дуг
            controlPoints.Add(new List<float> { values[0], values[1], values[2], values[3] });
            endPoints.Add(new List<float> { values[5], values[6] });
            controlPoints.Add(new List<float> { values[4] == 0 ? 1f : 0f });
        }

        return (controlPoints, endPoints);
    }
}