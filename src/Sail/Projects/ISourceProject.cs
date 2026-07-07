namespace Sail.Projects;

public interface ISourceProject
{
    Task PrepareAsync(BootstrapContext context);
    string ProjectPath { get; }
    string DisplayName { get; }

    // Whether this project is a native file-based application (a single `.cs` file run
    // directly via `dotnet run --file`, with no generated `.csproj`).
    bool IsFileBased => false;
}