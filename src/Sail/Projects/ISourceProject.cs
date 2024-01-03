namespace Sail.Projects;

public interface ISourceProject
{
    Task PrepareAsync(BootstrapContext context);
    string ProjectPath { get; }
    string DisplayName { get; }
}