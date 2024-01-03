
using System.Collections;

namespace Sail;

public record SailRunOptions(
    // DOTNET_SAIL_RUNNER
    string Runner,
    // DOTNET_SAIL_EXEC_NAME
    string? ExecName,
    // DOTNET_SAIL_SOURCE
    string? Source,
    // DOTNET_SAIL_CONFIGURATION
    string Configuration,
    // DOTNET_SAIL_LAUNCH_PROFILE
    string? LaunchProfile,
    // DOTNET_SAIL_ARGUMENTS
    string[]? Arguments,
    // DOTNET_SAIL_ENV_*
    IReadOnlyDictionary<string, string> EnvironmentVariables,
    // DOTNET_SAIL_SDK
    string? Sdk,
    // DOTNET_SAIL_TARGET_FRAMEWORK
    string? TargetFramework,
    // DOTNET_SAIL_VERBOSITY
    LogLevel Verbosity
)
{
    public const string Prefix = "DOTNET_SAIL_";

    public static SailRunOptions Default { get; } = new SailRunOptions(
        Runner: nameof(DotNetRunRunner),
        ExecName: null,
        Source: null,
        Configuration: "Release",
        LaunchProfile: null,
        Arguments: null,
        EnvironmentVariables: new Dictionary<string, string>(),
        Sdk: "Microsoft.NET.Sdk",
        TargetFramework: "net7.0",
        Verbosity: LogLevel.Information
    );

    public static SailRunOptions CreateFromEnvironmentVariables() =>
        CreateFromDictionary(Environment.GetEnvironmentVariables()
            .OfType<DictionaryEntry>()
            .Where(x => x is { Key: string, Value: string })
            .ToDictionary(k => (string)k.Key!, v => (string?)v.Value));

    public static SailRunOptions CreateFromDictionary(IReadOnlyDictionary<string, string?> dictionary)
    {
        var options = SailRunOptions.Default;
        options = options with
        {
            Runner = dictionary.GetValueOrDefault($"{Prefix}RUNNER", options.Runner)!,
            ExecName = dictionary.GetValueOrDefault($"{Prefix}EXEC_NAME", options.ExecName)!,
            Source = dictionary.GetValueOrDefault($"{Prefix}SOURCE", options.Source),
            Configuration = dictionary.GetValueOrDefault($"{Prefix}CONFIGURATION", options.Configuration)!,
            LaunchProfile = dictionary.GetValueOrDefault($"{Prefix}LAUNCH_PROFILE", options.LaunchProfile)!,
            Arguments = dictionary.GetValueOrDefault($"{Prefix}ARGUMENTS")?.Split(' ') ?? options.Arguments,
            Sdk = dictionary.GetValueOrDefault($"{Prefix}SDK", options.Sdk),
            TargetFramework = dictionary.GetValueOrDefault($"{Prefix}TARGET_FRAMEWORK", options.TargetFramework),
            Verbosity = ParseLogLevel(dictionary.GetValueOrDefault($"{Prefix}VERBOSITY")) ?? options.Verbosity,
        };

        var envVars = options.EnvironmentVariables.ToDictionary();
        foreach (var  envVar in dictionary.Where(x => x.Key.StartsWith($"{Prefix}ENV_") && !string.IsNullOrWhiteSpace(x.Key)))
        {
            envVars[envVar.Key.Substring($"{Prefix}ENV_".Length)] = envVar.Value!;
        }

        options = options with { EnvironmentVariables = envVars };

        return options;
    }

    public static SailRunOptions UpdateFromCommandLineArguments(SailRunOptions options, IReadOnlyList<string> args)
    {
        string[]? commandArguments = null;
        string? source = options.Source;
        var envVars = new Dictionary<string, string>(options.EnvironmentVariables);

        for (var i = 0; i < args.Count; i++)
        {
            if (source is null)
            {
                if (args[i] == "--help") continue; // Skip `--help`

                if (args[i].StartsWith('-') && args.Count >= i + 2)
                {
                    // Treat as Sail's options until the source argument is encountered.
                    var optionName = args[i];
                    var optionValue = args[i + 1];
                    switch (optionName)
                    {
                        case "-e":
                        case "--env":
                            var parts = optionValue.Split('=', 2);
                            envVars[parts[0]] = parts.Length > 1 ? parts[1] : string.Empty;
                            break;
                        case "-r":
                        case "--runner":
                            options = options with { Runner = optionValue };
                            break;
                        case "-c":
                        case "--configuration":
                            options = options with { Configuration = optionValue };
                            break;
                        case "-lp":
                        case "--launch-profile":
                            options = options with { LaunchProfile = optionValue };
                            break;
                        case "-s":
                        case "--source":
                            source = optionValue;
                            break;
                        case "-v":
                        case "--verbosity":
                            options = options with { Verbosity = ParseLogLevel(optionValue) ?? options.Verbosity };
                            break;
                        case "--exec-name":
                            options = options with { ExecName = optionValue };
                            break;
                        case "--sdk":
                            options = options with { Sdk = optionValue };
                            break;
                        case "--target-framework":
                            options = options with { TargetFramework = optionValue };
                            break;
                    }

                    i++;
                }
                else
                {
                    // [sail options...] [source (https://...)] [args...]
                    source = args[i];
                }
            }
            else
            {
                // sail's options are consumed. we treat the remain args as the command-line arguments.
                commandArguments = args.Skip(i).ToArray();
                break;
            }
        }

        options = options with
        {
            Source = source ?? options.Source,
            Arguments = commandArguments ?? options.Arguments,
            EnvironmentVariables = envVars, // DOTNET_SAIL_ENV + options
        };

        return options;
    }

    public static LogLevel? ParseLogLevel(string? value)
    {
        return value?.ToLowerInvariant() switch
        {
            "trace" or "t" => LogLevel.Trace,
            "information" or "i" or "info" => LogLevel.Information,
            "error" or "e" => LogLevel.Error,
            "none" or "n" => LogLevel.None,
            _ => null,
        };
    }
}
