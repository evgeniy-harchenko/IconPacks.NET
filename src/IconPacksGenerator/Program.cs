using IconPacksGenerator.IconGenerators;
using IconPacksGenerator.Services;

namespace IconPacksGenerator;

internal static class Program
{
    private static string? _apiKey = string.Empty;

    private static async Task Main(string?[] args)
    {
        if (args.Length > 0 && !string.IsNullOrEmpty(args[0]))
        {
            _apiKey = args[0];
        }

        FontRepositoryInfo featherFontRepositoryInfo = new FontRepositoryInfo()
        {
            WorkPath = Paths.FeatherIconPath,
            Remote = "https://github.com/feathericons/feather.git",
            Branch = "main",
            SparseCheckout = "icons/"
        };

        FontRepositoryInfo fontAwesomeFontRepositoryInfo = new FontRepositoryInfo()
        {
            WorkPath = Paths.FontAwesomeIconPath,
            Remote = "https://github.com/FortAwesome/Font-Awesome.git",
            Branch = "6.x",
            SparseCheckout = "metadata/icons.json"
        };

        FontRepositoryInfo ionicFontRepositoryInfo = new FontRepositoryInfo()
        {
            WorkPath = Paths.IonicIconPath,
            Remote = "https://github.com/ionic-team/ionicons.git",
            Branch = "main",
            SparseCheckout = "src/svg/"
        };

        FontRepositoryInfo materialFontRepositoryInfo = new FontRepositoryInfo()
        {
            WorkPath = Paths.MaterialIconPath,
            Remote = "https://github.com/google/material-design-icons.git",
            Branch = "master",
            SparseCheckout = "symbols/android/*/materialsymbolssharp/*_24px.xml"
        };

        FontRepositoryInfo materialCommunityFontRepositoryInfo = new FontRepositoryInfo()
        {
            WorkPath = Paths.MaterialCommunityIconPath,
            Remote = "https://github.com/Templarian/MaterialDesign.git",
            Branch = "master",
            SparseCheckout = "svg/"
        };

        FontRepositoryInfo simpleFontRepositoryInfo = new FontRepositoryInfo()
        {
            WorkPath = Paths.SimpleIconPath,
            Remote = "https://github.com/simple-icons/simple-icons.git",
            Branch = "develop",
            SparseCheckout = "icons/"
        };

        FontRepositoryInfo tablerFontRepositoryInfo = new FontRepositoryInfo()
        {
            WorkPath = Paths.TablerIconPath,
            Remote = "https://github.com/tabler/tabler-icons.git",
            Branch = "main",
            SparseCheckout = "icons/"
        };

        Console.WriteLine("Icons initializing...");

        FeatherGenerator featherGenerator = new FeatherGenerator(featherFontRepositoryInfo);
        await featherGenerator.InitIcons();

        FontAwesomeGenerator fontAwesomeGenerator = new FontAwesomeGenerator(fontAwesomeFontRepositoryInfo);
        await fontAwesomeGenerator.InitIcons();

        IonicGenerator ionicGenerator = new IonicGenerator(ionicFontRepositoryInfo);
        await ionicGenerator.InitIcons();

        MaterialGenerator materialGenerator = new MaterialGenerator(materialFontRepositoryInfo);
        await materialGenerator.InitIcons();

        MaterialCommunityGenerator materialCommunityGenerator = new MaterialCommunityGenerator(materialCommunityFontRepositoryInfo);
        await materialCommunityGenerator.InitIcons();

        SimpleGenerator simpleGenerator = new SimpleGenerator(simpleFontRepositoryInfo);
        await simpleGenerator.InitIcons();

        TablerGenerator tablerGenerator = new TablerGenerator(tablerFontRepositoryInfo);
        await tablerGenerator.InitIcons();

        /*Console.WriteLine();
        Console.WriteLine("Icons updating...");

        await featherGenerator.UpdateIcons();
        await fontAwesomeGenerator.UpdateIcons();
        await ionicGenerator.UpdateIcons();
        await materialGenerator.UpdateIcons();
        await materialCommunityGenerator.UpdateIcons();
        await simpleGenerator.UpdateIcons();
        await tablerGenerator.UpdateIcons();*/

        Console.WriteLine();
        Console.WriteLine("Generator running...");

        featherGenerator.Generate();
        fontAwesomeGenerator.Generate();
        ionicGenerator.Generate();
        materialGenerator.Generate();
        materialCommunityGenerator.Generate();
        simpleGenerator.Generate();
        tablerGenerator.Generate();

        Console.WriteLine();
        Console.WriteLine("Packs building...");

        await new NugetService(_apiKey).BuildIconPacks();

        Console.WriteLine();
        Console.WriteLine("Done!");
    }
}