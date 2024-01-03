using Sail.Projects;
using Sail.SourceProviders;
using Sail.Workspaces;

namespace Sail;

internal static class SailBootstrapper
{
    public static async Task RunAsync(string[] args)
    {
        var options = SailRunOptions.CreateFromEnvironmentVariables();
        try
        {
            options = SailRunOptions.UpdateFromCommandLineArguments(options, args);
            if (string.IsNullOrWhiteSpace(options.Source) || (args.Any() && args[0] == "--help"))
            {
                ShowHelp();
                Environment.ExitCode = 1;
                return;
            }

            var logger = new Logger(options.Verbosity);
            logger.Information($"Source: {options.Source}");

            using var workspace = CreateWorkspace(logger);
            var fetchResult = await FetchSourceAsync(options.Source, workspace, logger);
            var project = GetProject(workspace, fetchResult.TargetPath);

            var context = new BootstrapContext(options.Source, fetchResult.TargetPath, logger, options, workspace);
            Environment.ExitCode = await RunProjectAsync(context, project);
        }
        catch (SailExecutionException e)
        {
            Console.Error.WriteLine(e.Message);
            Environment.ExitCode = 1;
        }
    }

    private static void ShowHelp()
    {
        Console.Error.WriteLine($"""
             Usage: dotnet-sail [Options...] <SourceUrl> [Arguments...]

             Arguments:
               <SourceUrl>       Gist, GitHub, or any other address that provides source code. (https://, git://...)
               [Arguments...]    The arguments to be passed to the application.
                                 Environment variable `DOTNET_SAIL_ARGUMENTS`.

             Options:
               -s, --source <value>           Gist, GitHub, or any other address that provides source code. (https://, git://...)
                                              Can be used instead of passing it as an argument.
                                              Environment variable `DOTNET_SAIL_SOURCE`.
               -e, --env <value>              The environment variables for the application to be run.
                                              Environment variable `DOTNET_SAIL_ENV_*`.
               -v, --verbosity <value>        Set the verbosity level. The default is '{SailRunOptions.Default.Verbosity}'.
                                              Allowed values are `None`, `Error`, `Information` and `Trace`.
                                              Environment variable `DOTNET_SAIL_VERBOSITY`.
               -r, --runner <value>           The strategy to run for. The default is '{SailRunOptions.Default.Runner}'. Available runners are `run`, `publish`.
                                              Environment variable `DOTNET_SAIL_RUNNER`.
               -c, --configuration <value>    The configuration to run for. The default is '{SailRunOptions.Default.Configuration}'.
                                              Environment variable `DOTNET_SAIL_CONFIGURATION`.
               --launch-profile <value>       The name of the launch profile when the application launching with 'dotnet run' (DotNetRunRunner).
                                              The default is same as `--no-launch-profile`.
                                              Environment variable `DOTNET_SAIL_LAUNCH_PROFILE`.
               --exec-name <value>            The name of the entrypoint assembly when the application launching with 'dotnet exec' (DotNetPublishAndExecRunner).
                                              Environment variable `DOTNET_SAIL_EXEC_NAME`.
               --sdk <value>                  The SDK to run for single C# source project. The default is '{SailRunOptions.Default.Sdk}'.
                                              Environment variable `DOTNET_SAIL_SDK`.
               --target-framework <value>     The target framework to run for single C# source project. The default is '{SailRunOptions.Default.TargetFramework}'.
                                              Environment variable `DOTNET_SAIL_TARGET_FRAMEWORK`.

             Example:
               From Gist:
                 dotnet-sail https://gist.github.com/mayuki/d052d7457a63f25763ce8ecf04b1d0fc
               From GitHub.com:
                 dotnet-sail https://github.com/mayuki/dotnet-sail/tree/main/samples/SampleWebApp/SampleWebApp.csproj
               From Git repository:
                 dotnet-sail git://github.com/mayuki/dotnet-sail.git?branch=main&path=/main/samples/SampleWebApp/SampleWebApp.csproj
             """);
    }

    private static IWorkspace CreateWorkspace(Logger logger)
    {
        var workspace = new LocalTemporaryWorkspace();
        logger.Information($"Workspace: {workspace.RootDirectory}");
        logger.Trace($"SourceDirectory is '{workspace.SourceDirectory}'");
        logger.Trace($"ArtifactsDirectory is '{workspace.ArtifactsDirectory}'");
        Environment.CurrentDirectory = workspace.RootDirectory;
        logger.Trace($"CurrentDirectory is '{Environment.CurrentDirectory}'.");

        return workspace;
    }

    private static async Task<SourceProviderResult> FetchSourceAsync(string source, IWorkspace workspace, Logger logger)
    {
        var sourceProviderContext = new SourceProviderContext(source, workspace, logger);
        if (!SourceProviderResolver.Default.TryGetProvider(sourceProviderContext, out var sourceProvider))
        {
            throw new SailExecutionException("No source provider matched.");
        }

        logger.Information($"Using source provider is '{sourceProvider.GetType().Name}'.");
        var fetchResult = await sourceProvider.FetchAndExtractToWorkspaceAsync(sourceProviderContext);
        if (fetchResult.TargetPath is { } targetPath && targetPath.StartsWith("/"))
        {
            throw new InvalidOperationException($"The TargetPath returned by SourceProvider must not start with '/'. (TargetPath={targetPath})");
        }
        logger.Trace($"TargetPath is '{fetchResult.TargetPath ?? "(null)"}'.");

        return fetchResult;
    }

    private static ISourceProject GetProject(IWorkspace workspace, string? targetPath)
    {
        if (!ProjectResolver.TryFindProjects(workspace.SourceDirectory, targetPath, out var candidateProjects))
        {
            throw new SailExecutionException($"No project found in the source. (sourceDirectory={workspace.SourceDirectory}; targetPath={targetPath})");
        }
        if (candidateProjects.Count > 1)
        {
            throw new SailExecutionException($"Multiple projects found: {string.Join(";", candidateProjects)}.");
        }

        return candidateProjects.Single();
    }

    private static async Task<int> RunProjectAsync(BootstrapContext context, ISourceProject project)
    {
        context.Logger.Information($"{project.DisplayName} ({project.ProjectPath}) found.");
        await project.PrepareAsync(context);

        if (!ProjectRunnerResolver.Default.TryGetRunner(context.Options.Runner, out var runner))
        {
            throw new SailExecutionException($"Specified project runner '{context.Options.Runner}' is not found.");
        }

        context.Logger.Information($"Project runner is '{runner.GetType().Name}'.");

        var exitCode = await runner.RunAsync(context, project);
        context.Logger.Information($"The program has exited with exit code '{exitCode}'.");
        return exitCode;
    }
}