using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Sail.Projects;

namespace Sail;

public interface IProjectRunner
{
    IReadOnlyList<string> Aliases { get; }
    Task<int> RunAsync(BootstrapContext context, ISourceProject project);
}

public class ProjectRunnerResolver(IReadOnlyList<IProjectRunner> runners)
{
    public static ProjectRunnerResolver Default { get; } = new ProjectRunnerResolver(
        [
            new DotNetRunRunner(),
            new DotNetPublishAndExecRunner(),
        ]);

    public bool TryGetRunner(string name, [NotNullWhen(true)] out IProjectRunner? runner)
    {
        foreach (var candidateRunner in runners)
        {
            if (string.Equals(candidateRunner.GetType().Name, name, StringComparison.OrdinalIgnoreCase) || candidateRunner.Aliases.Any(x => string.Equals(x, name, StringComparison.OrdinalIgnoreCase)))
            {
                runner = candidateRunner;
                return true;
            }
        }

        runner = null;
        return false;
    }
}

public class DotNetRunRunner : IProjectRunner
{
    public IReadOnlyList<string> Aliases { get; } = ["run"];

    public async Task<int> RunAsync(BootstrapContext context, ISourceProject project)
    {
        string[] buildArgs = [project.ProjectPath];

        if (context.Options.Configuration is not null)
        {
            buildArgs = [.. buildArgs, "--configuration", context.Options.Configuration];
        }

        context.Logger.Information($"Run dotnet: dotnet build {string.Join(' ', buildArgs)}");
        {
            var procStartInfo = new ProcessStartInfo("dotnet", ["build", .. buildArgs]);
            using var proc = Process.Start(procStartInfo) ?? throw new SailExecutionException($"Failed to launch a dotnet process."); ;
            await proc.WaitForExitAsync();
            if (proc.ExitCode != 0)
            {
                throw new SailExecutionException($"The dotnet process exited with exit code '{proc.ExitCode}'.");
            }
        }

        var args = CreateDotNetRunArguments(context, project);
        context.Logger.Information($"Run dotnet: dotnet run {string.Join(' ', args)}");
        {
            var procStartInfo = new ProcessStartInfo("dotnet", ["run", .. args]);
            foreach (var envVar in context.Options.EnvironmentVariables)
            {
                context.Logger.Trace($"EnvVar: {envVar.Key} = {envVar.Value}");
                procStartInfo.Environment.Add(envVar.Key, envVar.Value);
            }
            using var proc = Process.Start(procStartInfo) ?? throw new SailExecutionException($"Failed to launch a dotnet process."); ;
            await proc.WaitForExitAsync();
            return proc.ExitCode;
        }
    }

    private static IReadOnlyList<string> CreateDotNetRunArguments(BootstrapContext context, ISourceProject project)
    {
        string[] args = ["--no-build", "--project", project.ProjectPath];

        if (context.Options.LaunchProfile is not null)
        {
            args = [.. args, "--launch-profile", context.Options.LaunchProfile];
        }
        else if (context.Options.NoLaunchProfile ?? false)
        {
            args = [.. args, "--no-launch-profile"];
        }

        if (context.Options.Arguments is not null)
        {
            args = [.. args, .. context.Options.Arguments];
        }

        return args.ToArray();
    }
}

public class DotNetPublishAndExecRunner : IProjectRunner
{
    public IReadOnlyList<string> Aliases { get; } = ["publish", "exec"];

    public async Task<int> RunAsync(BootstrapContext context, ISourceProject project)
    {
        string[] publishArgs = [project.ProjectPath, "--output", context.Workspace.ArtifactsDirectory];

        if (context.Options.Configuration is not null)
        {
            publishArgs = [.. publishArgs, "--configuration", context.Options.Configuration];
        }

        context.Logger.Information($"Run dotnet: dotnet publish {string.Join(' ', publishArgs)}");
        {
            var procStartInfo = new ProcessStartInfo("dotnet", ["publish", .. publishArgs]);
            using var proc = Process.Start(procStartInfo) ?? throw new SailExecutionException($"Failed to launch a dotnet process."); ;
            await proc.WaitForExitAsync();
            if (proc.ExitCode != 0)
            {
                throw new SailExecutionException($"The dotnet process exited with exit code '{proc.ExitCode}'.");
            }
        }

        var entryAssemblyName = context.Options.ExecName ?? Path.GetFileNameWithoutExtension(project.ProjectPath) + ".dll";
        string[] execArgs = [Path.Combine(context.Workspace.ArtifactsDirectory, entryAssemblyName), .. (context.Options.Arguments ?? Array.Empty<string>())];
        context.Logger.Information($"Run dotnet: dotnet exec {string.Join(' ', execArgs)}");
        {
            var procStartInfo = new ProcessStartInfo("dotnet", ["exec", .. execArgs])
            {
                // Change the current directory to the directory for artifacts.
                WorkingDirectory = context.Workspace.ArtifactsDirectory,
            };
            foreach (var envVar in context.Options.EnvironmentVariables)
            {
                context.Logger.Trace($"EnvVar: {envVar.Key} = {envVar.Value}");
                procStartInfo.Environment.Add(envVar.Key, envVar.Value);
            }
            using var proc = Process.Start(procStartInfo) ?? throw new SailExecutionException($"Failed to launch a dotnet process."); ;
            await proc.WaitForExitAsync();
            return proc.ExitCode;
        }
    }
}