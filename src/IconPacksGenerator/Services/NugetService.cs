using System.Text;
using CliWrap;
using CliWrap.Buffered;

namespace IconPacksGenerator.Services;

public class NugetService
{
    private string? _apiKey;
    private readonly string[] _projectPaths;

    public NugetService(string? apiKey)
    {
        _apiKey = apiKey;
        _projectPaths = Directory.GetDirectories(Paths.RootPath, "IconPacks.*");
    }

    internal async Task BuildIconPacks()
    {
        var oldNupkgs = Directory.EnumerateFiles(
            Paths.RootPath,
            "*.nupkg",
            SearchOption.AllDirectories
        );
        foreach (var nupkg in oldNupkgs)
        {
            File.Delete(nupkg);
        }

        foreach (var projectPath in _projectPaths)
        {
            var projectName = Path.GetFileName(projectPath);
            await PackProjectAsync(projectName);
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            Console.WriteLine("Please input you nuget api-key...");
            _apiKey = Console.ReadLine();
        }

        if (string.IsNullOrEmpty(_apiKey))
        {
            Console.WriteLine("Error: Nuget api-key is empty!");
            return;
        }

        var newNupkgs = Directory.EnumerateFiles(
            Paths.RootPath,
            "*.nupkg",
            SearchOption.AllDirectories
        );

        foreach (var nupkg in newNupkgs)
        {
            await Cli.Wrap("dotnet")
                .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine, Encoding.UTF8))
                .WithArguments(
                    $"nuget push {nupkg} --api-key {_apiKey} --source https://api.nuget.org/v3/index.json"
                )
                .ExecuteBufferedAsync();
        }
    }

    private async Task PackProjectAsync(string projectName)
    {
        Console.Write("Packing {0}: ", projectName);
        try
        {
            await Cli.Wrap("dotnet")
                .WithWorkingDirectory(Paths.RootPath)
                .WithArguments($"pack ./{projectName} -c release")
                .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine, Encoding.UTF8))
                .ExecuteBufferedAsync();
            Console.WriteLine("done.");
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to pack: \n{0} ", e);
        }
    }
}