namespace Sail.Tests;

internal class TemporaryDirectory : IDisposable
{
    public string DirectoryPath { get; }

    public TemporaryDirectory()
    {
        var tmpPath = Path.GetTempFileName();
        File.Delete(tmpPath);
        DirectoryPath = tmpPath;
        Directory.CreateDirectory(tmpPath);
    }

    public void AddFile(string path, string content)
        => File.WriteAllText(CombinePathAndEnsureDirectory(DirectoryPath, path), content);

    private string CombinePathAndEnsureDirectory(string path1, string path2)
    {
        var combined = Path.Combine(path1, path2);
        if (!Directory.Exists(Path.GetDirectoryName(combined)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(combined)!);
        }
        return combined;
    }

    public void Dispose()
    {
        if (DirectoryPath.StartsWith(Path.GetTempPath()))
        {
            try
            {
                Directory.Delete(DirectoryPath, recursive: true);
            }
            catch
            {
            }
        }
    }
}