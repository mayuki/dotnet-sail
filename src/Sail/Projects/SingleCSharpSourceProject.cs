namespace Sail.Projects;

public class SingleCSharpSourceProject(string sourcePath) : ISourceProject
{
    private const string CsProjFileName = "App.csproj";
    public string DisplayName => "Single source C# project";
    public string ProjectPath { get; } = Path.Combine(Path.GetDirectoryName(sourcePath)!, CsProjFileName);
    public string SourcePath { get; } = sourcePath;

    public async Task PrepareAsync(BootstrapContext context)
    {
        if (Directory.EnumerateFiles(context.Workspace.SourceDirectory, "*.csproj").Any())
        {
            throw new SailExecutionException("Some project already exists in the directory.");
        }

        var targetFramework = context.Options.TargetFramework ?? "net8.0";
        var sdk = context.Options.Sdk ?? "Microsoft.NET.Sdk";

        var csProjPath = Path.Combine(context.Workspace.SourceDirectory, CsProjFileName);
        context.Logger.Information($"Write '{csProjPath}'. (sdk={sdk}; targetFramework={targetFramework})");
        var csproj = $"""
                      <Project Sdk="{sdk}">
                        <PropertyGroup>
                          <OutputType>Exe</OutputType>
                          <TargetFramework>{targetFramework}</TargetFramework>
                          <ImplicitUsings>enable</ImplicitUsings>
                        </PropertyGroup>
                      </Project>
                      """;
        await File.WriteAllTextAsync(Path.Combine(context.Workspace.SourceDirectory, CsProjFileName), csproj);
    }

    public override string ToString() => $"{DisplayName} ({ProjectPath})";
}