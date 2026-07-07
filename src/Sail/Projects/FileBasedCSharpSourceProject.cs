namespace Sail.Projects;

// Represents a single `.cs` file run through the native .NET file-based application
// path (`dotnet run --file`). No `App.csproj` is generated for this project kind.
public class FileBasedCSharpSourceProject(string sourcePath) : ISourceProject
{
    public string DisplayName => "File-based C# application";
    public string ProjectPath { get; } = sourcePath;
    public bool IsFileBased => true;

    public Task PrepareAsync(BootstrapContext context) => Task.CompletedTask;

    public override string ToString() => $"{DisplayName} ({ProjectPath})";
}
