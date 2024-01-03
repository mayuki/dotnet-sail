using Sail.Workspaces;

namespace Sail;

public class BootstrapContext(string source, string? targetPath, Logger logger, SailRunOptions options, IWorkspace workspace)
{
    public string Source => source;
    public string? TargetPath { get; set; } = targetPath;
    public IWorkspace Workspace => workspace;

    public SailRunOptions Options { get; set; } = options;
    public Logger Logger { get; } = logger;
}