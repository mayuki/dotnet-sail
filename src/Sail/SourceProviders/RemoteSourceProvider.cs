using System.IO.Compression;

namespace Sail.SourceProviders;

public class RemoteSourceProvider : ISourceProvider
{
    public bool CanHandle(SourceProviderContext context)
        => GeneratedRegex.RemoteUrl().IsMatch(context.Source);

    public async Task<SourceProviderResult> FetchAndExtractToWorkspaceAsync(SourceProviderContext context)
    {
        var url = context.Source;
        context.Logger.Information($"Downloading '{url}' ...");

        var request = new HttpRequestMessage(HttpMethod.Get, context.Source);
        request.Headers.Add("Accept", "application/x-zip-compressed, text/plain, */*;q=0.8");
        request.Headers.Add("User-Agent", "dotnet-sail/1.0");

        using var httpClient = new HttpClient();
        var response = await httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        context.Logger.Trace($"Response Content-Type is '{response.Content.Headers.ContentType}'");
        var extContentType = response.Content.Headers.ContentType?.MediaType switch
        {
            "text/plain" => "cs",
            "application/x-zip-compressed" => "zip",
            "application/zip" => "zip",
            _ => "bin",
        };

        var tmpPath = Path.Combine(context.Workspace.SourceDirectory, $"download.{extContentType}");
        context.Logger.Trace($"Write to '{tmpPath}'");
        using (var downloadedFile = File.Create(tmpPath))
        {
            await response.Content.CopyToAsync(downloadedFile);
        }

        var destDirectory = Path.Combine(context.Workspace.SourceDirectory, "d");
        Directory.CreateDirectory(destDirectory);
        var isZip = false;
        if (extContentType == "bin")
        {
            // Detect a filetype from the downloaded file.
            var fileInfo = new FileInfo(tmpPath);
            if (fileInfo.Length > 2)
            {
                var buffer = new byte[2];
                using (var stream = File.OpenRead(tmpPath))
                {
                    stream.ReadExactly(buffer);
                }
                if (buffer.AsSpan().SequenceEqual("PK"u8))
                {
                    // The file is Zip.
                    context.Logger.Trace($"The file '{tmpPath}' is seems to be a zip file.");
                    isZip = true;
                }
            }
        }
        else if (extContentType == "zip")
        {
            isZip = true;
        }

        if (isZip)
        {
            context.Logger.Trace($"Unzipping '{tmpPath}' to '{destDirectory}'");
            using var stream = File.OpenRead(tmpPath);
            using var zipArchive = new ZipArchive(stream);
            zipArchive.ExtractToDirectory(destDirectory);
            return new SourceProviderResult(Path.Combine("d"));
        }
        else
        {
            var destPath = Path.Combine(destDirectory, "Program.cs");
            context.Logger.Trace($"Move '{tmpPath}' to '{destPath}'");
            File.Move(tmpPath, destPath);

            return new SourceProviderResult(Path.Combine("d"));
        }
    }
}
