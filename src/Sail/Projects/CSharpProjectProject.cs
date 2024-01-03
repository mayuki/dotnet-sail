namespace Sail.Projects;

public class CSharpProjectProject(string projectPath) : ISourceProject
{
    public string DisplayName => "C# Project";
    public string ProjectPath { get; } = projectPath;
    public Task PrepareAsync(BootstrapContext context) => Task.CompletedTask;

    public override string ToString() => $"{DisplayName} ({ProjectPath})";
}