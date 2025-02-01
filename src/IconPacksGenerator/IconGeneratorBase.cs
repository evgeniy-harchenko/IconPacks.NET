using IconPacksGenerator.Services;

namespace IconPacksGenerator;

internal abstract class IconGeneratorBase
{
    private readonly FontRepositoryInfo _fontRepositoryInfo;
    private readonly GitService _gitService;
    protected readonly Dictionary<string, string> IconKinds = new Dictionary<string, string>();
    private bool _isReadyForGeneration = false;

    protected abstract string RootPath { get; }
    protected abstract string Type { get; }

    protected internal IconGeneratorBase(FontRepositoryInfo fontRepositoryInfo)
    {
        _fontRepositoryInfo = fontRepositoryInfo;
        _gitService = new GitService(fontRepositoryInfo.WorkPath);
    }

    internal async Task InitIcons()
    {
        _isReadyForGeneration = false;

        Console.Write("Initializing {0}: ", Type);
        try
        {
            if (!Directory.Exists(_fontRepositoryInfo.WorkPath))
            {
                Directory.CreateDirectory(_fontRepositoryInfo.WorkPath);
            }

            await _gitService.InitializeRepository(_fontRepositoryInfo.Remote, _fontRepositoryInfo.SparseCheckout);
            await _gitService.PullBranch(_fontRepositoryInfo.Remote, _fontRepositoryInfo.Branch);

            _isReadyForGeneration = true;
        }
        catch (Exception e)
        {
            _isReadyForGeneration = false;
            Console.WriteLine("Failed to initialize: \n{0} ", e);
        }
    }

    internal async Task UpdateIcons()
    {
        _isReadyForGeneration = false;

        Console.Write("Updating {0}: ", Type);
        try
        {
            await _gitService.UpdateBranch(_fontRepositoryInfo.Branch, _fontRepositoryInfo.SparseCheckout);

            _isReadyForGeneration = true;
        }
        catch (Exception e)
        {
            _isReadyForGeneration = false;
            Console.WriteLine("Failed to update: \n{0} ", e);
        }
    }

    internal void Generate()
    {
        Console.Write("Generating {0}: ", Type);

        if (!_isReadyForGeneration)
        {
            Console.WriteLine("Nothing to generate.");
            return;
        }

        try
        {
            Generation();
            Console.WriteLine("done.");
        }
        catch (Exception e)
        {
            Console.WriteLine("Failed to generate icons: \n{0} ", e);
        }
    }

    protected virtual void Generation()
    {
        var files = Directory.EnumerateFiles(RootPath, "*.svg");
        foreach (var svgFile in files)
        {
            ProcessSvgFile(svgFile);
        }

        Util.OutputIconKindFile(IconKinds, Type);

        IconPdfGenerator.GeneratePdf(Type, IconKinds);
    }

    private void ProcessSvgFile(string filePath)
    {
        var id = Path.GetFileNameWithoutExtension(filePath);
        var data = Util.GetSvgData(filePath);
        if (!string.IsNullOrEmpty(data))
        {
            IconKinds.Add(id.GetCamelId(), data);
        }
    }
}