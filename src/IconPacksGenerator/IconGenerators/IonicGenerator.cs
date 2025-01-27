namespace IconPacksGenerator.IconGenerators;

internal class IonicGenerator(FontRepositoryInfo fontRepositoryInfo) : IconGeneratorBase(fontRepositoryInfo)
{
    protected override string RootPath => Path.Combine(Paths.IonicIconPath, "./src/svg/");
    protected override string Type => "Ionic";
}