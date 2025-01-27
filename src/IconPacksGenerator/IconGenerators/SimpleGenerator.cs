namespace IconPacksGenerator.IconGenerators;

internal class SimpleGenerator(FontRepositoryInfo fontRepositoryInfo) : IconGeneratorBase(fontRepositoryInfo)
{
    protected override string RootPath => Path.Combine(Paths.SimpleIconPath, "./icons/");
    protected override string Type => "Simple";
}