namespace IconPacksGenerator.IconGenerators;

internal class MaterialCommunityGenerator(FontRepositoryInfo fontRepositoryInfo) : IconGeneratorBase(fontRepositoryInfo)
{
    protected override string RootPath => Path.Combine(Paths.MaterialCommunityIconPath, "./svg/");
    protected override string Type => "MaterialCommunity";
}