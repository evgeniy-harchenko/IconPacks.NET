using IconPacksGenerator.Services;

namespace IconPacksGenerator.IconGenerators;

internal class MaterialGenerator(FontRepositoryInfo fontRepositoryInfo) : IconGeneratorBase(fontRepositoryInfo)
{
    protected override string RootPath => Path.Combine(Paths.MaterialIconPath, "./symbols/android/");
    protected override string Type => "Material";

    protected override void Generation()
    {
        foreach (var category in Directory.EnumerateDirectories(RootPath))
        {
            var id = Path.GetFileNameWithoutExtension(category);
            var path = Path.Combine(category, "materialsymbolssharp", $"{id}_24px.xml");
            var data = Util.GetXmlData(path);
            if (!string.IsNullOrEmpty(data))
            {
                IconKinds.Add(id.GetCamelId(), data);
            }
        }

        Util.OutputIconKindFile(IconKinds, Type);
        
        IconPdfGenerator.GeneratePdf(Type, IconKinds);
    }
}
