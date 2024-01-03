using System.Diagnostics;
using Sail.SourceProviders;

namespace Sail;

public static class ProcessHelper
{
    public static Task StartProcessAsync(BootstrapContext context, string fileName, IReadOnlyList<string> arguments)
        => StartProcessAsync(context.Workspace.SourceDirectory, fileName, arguments, context.Logger);
    public static Task StartProcessAsync(SourceProviderContext context, string fileName, IReadOnlyList<string> arguments)
        => StartProcessAsync(context.Workspace.SourceDirectory, fileName, arguments, context.Logger);

    public static async Task StartProcessAsync(string workspaceDirectory, string fileName, IReadOnlyList<string> arguments, Logger logger)
    {
        logger.Trace($"Execute command '{fileName}' with arguments '{string.Join(' ', arguments)}'.");
        var procStartInfo = new ProcessStartInfo(fileName, arguments);
        procStartInfo.WorkingDirectory = workspaceDirectory;
        var proc = Process.Start(procStartInfo) ?? throw new SailExecutionException($"Failed to launch a process: '{fileName}'");
        await proc.WaitForExitAsync();
        if (proc.ExitCode != 0)
        {
            throw new SailExecutionException($"Command '{fileName}' has exited with exit code '{proc.ExitCode}'.");
        }
    }
}