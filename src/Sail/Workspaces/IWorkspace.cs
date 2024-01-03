namespace Sail.Workspaces;

public interface IWorkspace : IDisposable
{
    string RootDirectory { get; }
    string SourceDirectory { get; }
    string ArtifactsDirectory { get; }
}