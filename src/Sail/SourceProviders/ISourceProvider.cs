using Sail.Workspaces;

namespace Sail.SourceProviders;

public interface ISourceProvider
{
    Task<SourceProviderResult> FetchAndExtractToWorkspaceAsync(SourceProviderContext context);
    bool CanHandle(SourceProviderContext context);
}

public record SourceProviderContext(string Source, IWorkspace Workspace, Logger Logger);
public record SourceProviderResult(string? TargetPath = null);