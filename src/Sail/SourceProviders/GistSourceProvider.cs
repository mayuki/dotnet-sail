using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sail.SourceProviders;

public class GistSourceProvider : ISourceProvider
{
    public bool CanHandle(SourceProviderContext context)
        => GeneratedRegex.GistUrl().IsMatch(context.Source);

    public async Task<SourceProviderResult> FetchAndExtractToWorkspaceAsync(SourceProviderContext context)
    {
        var url = context.Source;
        var match = GeneratedRegex.GistUrl().Match(url);
        var gistId = match.Groups["gistId"].Value;
        var revision = match.Groups["revision"].Success ? match.Groups[2].Value : null;
        context.Logger.Information($"Fetching source codes from Gist '{url}' (revision:{(string.IsNullOrWhiteSpace(revision) ? "latest" : revision)}) ...");
        var apiUrl = $"https://api.github.com/gists/{gistId}";
        context.Logger.Trace($"Fetching '{apiUrl}' ...");
        using var httpClient = new HttpClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "dotnet-sail/1.0");
        httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        httpClient.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");

        var request = new HttpRequestMessage(HttpMethod.Get, apiUrl);
        var response = await httpClient.SendAsync(request);

        var json = await response.Content.ReadAsStringAsync();
        var gistInfo = JsonSerializer.Deserialize<GistInfo>(json, GistJsonSourceGenerationContext.Default.GistInfo);
        if (gistInfo is null) throw new SailExecutionException("Failed to fetch a gist information from the API.");

        foreach (var (fileName, fileInfo) in gistInfo.files)
        {
            var content = fileInfo.content;
            if (fileInfo.truncated)
            {
                context.Logger.Trace($"Fetching '{fileInfo.raw_url}' ...");
                content = await httpClient.GetStringAsync(fileInfo.raw_url);
            }
            context.Logger.Trace($"Write '{fileInfo.filename}'. (truncated={fileInfo.truncated}; type={fileInfo.type}; language={fileInfo.language})");
            await File.WriteAllTextAsync(Path.Combine(context.Workspace.SourceDirectory, fileInfo.filename), fileInfo.content, new UTF8Encoding(false));
        }

        return new SourceProviderResult(null);
    }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(GistInfo))]
public partial class GistJsonSourceGenerationContext : JsonSerializerContext;

public class GistInfo
{
    public required Dictionary<string, GistFile> files { get; set; }
}
public class GistFile
{
    public required string filename { get; set; }
    public required string type { get; set; }
    public required string language { get; set; }
    public required string raw_url { get; set; }
    public required string content { get; set; }
    public bool truncated { get; set; }
}