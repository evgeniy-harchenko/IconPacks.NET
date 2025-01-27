using System.Text.Json;

namespace IconPacksGenerator.IconGenerators;

internal class FontAwesomeGenerator(FontRepositoryInfo fontRepositoryInfo) : IconGeneratorBase(fontRepositoryInfo)
{
    protected override string RootPath => string.Empty;
    protected override string Type => "FontAwesome";

    private readonly string _iconsPath = Path.Combine(Paths.FontAwesomeIconPath, "./metadata/icons.json");

    protected override void Generation()
    {
        var icons = JsonSerializer.Deserialize<Dictionary<string, FontAwesomeIcon>>(
            new FileStream(_iconsPath, FileMode.Open)
        );

        if (icons != null)
        {
            foreach (var icon in icons)
            {
                var path = icon.Value?.Svg?.Solid?.Path;
                if (!string.IsNullOrEmpty(path))
                {
                    IconKinds.Add(icon.Key.GetCamelId(), path);
                }
            }
        }

        Util.OutputIconKindFile(IconKinds, Type);
    }
}