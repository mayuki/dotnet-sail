namespace Sail.SourceProviders;

public class GitHubSourceProvider : GitSourceProviderBase
{
    public override bool CanHandle(SourceProviderContext context)
        => GeneratedRegex.GitHubUrl().IsMatch(context.Source);

    public override async Task<SourceProviderResult> FetchAndExtractToWorkspaceAsync(SourceProviderContext context)
    {
        var match = GeneratedRegex.GitHubUrl().Match(context.Source);
        var cloneUrl = match.Groups["baseUrl"] + ".git";

        return await CloneAsync(context, cloneUrl, match.Groups["hashOrBranch"].Value, match.Groups["path"].Value);
    }
}