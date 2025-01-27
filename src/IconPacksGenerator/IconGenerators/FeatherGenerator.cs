namespace IconPacksGenerator.IconGenerators;

internal class FeatherGenerator(FontRepositoryInfo fontRepositoryInfo) : IconGeneratorBase(fontRepositoryInfo)
{
    protected override string RootPath => Path.Combine(Paths.FeatherIconPath, "./icons/");
    protected override string Type => "Feather";
}
