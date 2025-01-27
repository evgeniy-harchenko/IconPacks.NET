using System.Text;
using CliWrap;
using CliWrap.Buffered;

namespace IconPacksGenerator.Services;

internal class GitService
{
    private readonly string _workingDirectory;

    public GitService(string workingDirectory)
    {
        _workingDirectory = workingDirectory;
    }

    public async Task InitializeRepository(string remote, string sparseCheckout)
    {
        if (!Directory.Exists(Path.Combine(_workingDirectory, ".git")))
        {
            await Cli.Wrap("git")
                .WithWorkingDirectory(_workingDirectory)
                .WithArguments("init")
                .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine, Encoding.UTF8))
                .ExecuteBufferedAsync();
            await Cli.Wrap("git")
                .WithWorkingDirectory(_workingDirectory)
                .WithArguments($"remote add remote {remote}")
                .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine, Encoding.UTF8))
                .ExecuteBufferedAsync();
            await Cli.Wrap("git")
                .WithWorkingDirectory(_workingDirectory)
                .WithArguments("config core.sparsecheckout true")
                .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine, Encoding.UTF8))
                .ExecuteBufferedAsync();
            await File.WriteAllTextAsync(Path.Combine(_workingDirectory, ".git/info/sparse-checkout"),
                sparseCheckout);
        }
    }

    public async Task PullBranch(string remote, string branch)
    {
        if (Directory.Exists(Path.Combine(_workingDirectory, ".git")))
        {
            await Cli.Wrap("git")
                .WithWorkingDirectory(_workingDirectory)
                .WithArguments($"pull remote {branch}:master")
                .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine, Encoding.UTF8))
                .ExecuteBufferedAsync();
        }
    }

    public async Task UpdateBranch(string branch, string sparseCheckout)
    {
        if (Directory.Exists(Path.Combine(_workingDirectory, ".git")))
        {
            await Cli.Wrap("git")
                .WithWorkingDirectory(_workingDirectory)
                .WithArguments("config core.sparsecheckout true")
                .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine, Encoding.UTF8))
                .ExecuteBufferedAsync();
            await File.WriteAllTextAsync(Path.Combine(_workingDirectory, ".git/info/sparse-checkout"),
                sparseCheckout);
            await Cli.Wrap("git")
                .WithWorkingDirectory(_workingDirectory)
                .WithArguments($"pull remote {branch}:master")
                .WithStandardOutputPipe(PipeTarget.ToDelegate(Console.WriteLine, Encoding.UTF8))
                .ExecuteBufferedAsync();
        }
    }
}