namespace Sail.SourceProviders;

public class GitSourceProvider : GitSourceProviderBase
{
    public override bool CanHandle(SourceProviderContext context)
        => GeneratedRegex.GitUrl().IsMatch(context.Source);

    public override async Task<SourceProviderResult> FetchAndExtractToWorkspaceAsync(SourceProviderContext context)
    {
        var uri = new Uri(context.Source);
        var cloneUrl = uri.GetComponents(UriComponents.SchemeAndServer | UriComponents.Path, UriFormat.UriEscaped);
        var queryString = uri.Query.TrimStart('?').Split('&').Select(x => x.Split('=', 2)).ToDictionary(k => k[0], v => v[1]);
        var branchOrHash = queryString.GetValueOrDefault("branch") ?? queryString.GetValueOrDefault("hash");
        var path = queryString.GetValueOrDefault("path");

        return await CloneAsync(context, cloneUrl, branchOrHash, path);
    }
}

public abstract class GitSourceProviderBase : ISourceProvider
{
    protected async Task<SourceProviderResult> CloneAsync(SourceProviderContext context, string remoteUrl, string? branchOrHash, string? path)
    {
        if (!remoteUrl.EndsWith(".git")) throw new ArgumentException("remoteUrl must be end with '.git'");

        var targetPath = default(string);
        if (string.IsNullOrWhiteSpace(branchOrHash) && string.IsNullOrWhiteSpace(path))
        {
            context.Logger.Information($"Cloning from Git '{remoteUrl}' ...");
            await ProcessHelper.StartProcessAsync(context, "git", ["clone", "--depth=1", "--single-branch", remoteUrl, context.Workspace.SourceDirectory]);
        }
        else
        {
            context.Logger.Information($"Cloning from Git '{remoteUrl}' (branchOrHash={branchOrHash}; path={path})...");
            await ProcessHelper.StartProcessAsync(context, "git", ["init", "--initial-branch=main"]);
            await ProcessHelper.StartProcessAsync(context, "git", ["remote", "add", "origin", remoteUrl]);
            if (!string.IsNullOrWhiteSpace(branchOrHash))
            {
                await ProcessHelper.StartProcessAsync(context, "git", ["fetch", "--depth=1", "origin", branchOrHash]);
            }
            else
            {
                await ProcessHelper.StartProcessAsync(context, "git", ["fetch", "--depth=1", "origin"]);
            }
            await ProcessHelper.StartProcessAsync(context, "git", ["-c", "advice.detachedHead=false", "checkout", "FETCH_HEAD"]);

            if (!string.IsNullOrWhiteSpace(path))
            {
                targetPath = path.TrimStart('/', '\\');
            }
        }

        return new SourceProviderResult(targetPath);
    }

    public abstract Task<SourceProviderResult> FetchAndExtractToWorkspaceAsync(SourceProviderContext context);

    public abstract bool CanHandle(SourceProviderContext context);
}