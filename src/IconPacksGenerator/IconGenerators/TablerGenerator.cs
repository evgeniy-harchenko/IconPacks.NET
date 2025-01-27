namespace IconPacksGenerator.IconGenerators;

internal class TablerGenerator(FontRepositoryInfo fontRepositoryInfo) : IconGeneratorBase(fontRepositoryInfo)
{
    protected override string RootPath => Path.Combine(Paths.TablerIconPath, "./icons/outline/");
    protected override string Type => "Tabler";
}