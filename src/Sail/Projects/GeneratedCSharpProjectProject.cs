namespace Sail.Projects;

// Compatibility path for downloaded sources that contain multiple loose `.cs` files
// and no `.csproj`. A temporary conventional project targeting `net10.0` is generated
// so that the loose sources can be compiled together (SDK-style projects glob `*.cs`
// under the project directory automatically).
public class GeneratedCSharpProjectProject(string directoryPath) : ISourceProject
{
    private const string CsProjFileName = "App.csproj";
    private const string Sdk = "Microsoft.NET.Sdk";
    private const string TargetFramework = "net10.0";

    public string DisplayName => "Generated C# project";
    public string DirectoryPath { get; } = directoryPath;
    public string ProjectPath { get; } = Path.Combine(directoryPath, CsProjFileName);

    public async Task PrepareAsync(BootstrapContext context)
    {
        if (Directory.EnumerateFiles(DirectoryPath, "*.csproj").Any())
        {
            throw new SailExecutionException("Some project already exists in the directory.");
        }

        context.Logger.Information($"Write '{ProjectPath}'. (sdk={Sdk}; targetFramework={TargetFramework})");
        var csproj = $"""
                      <Project Sdk="{Sdk}">
                        <PropertyGroup>
                          <OutputType>Exe</OutputType>
                          <TargetFramework>{TargetFramework}</TargetFramework>
                          <ImplicitUsings>enable</ImplicitUsings>
                        </PropertyGroup>
                      </Project>
                      """;
        await File.WriteAllTextAsync(ProjectPath, csproj);
    }

    public override string ToString() => $"{DisplayName} ({ProjectPath})";
}
