namespace Sail.Workspaces;

public class LocalTemporaryWorkspace : IWorkspace
{
    public string RootDirectory { get; }
    public string SourceDirectory { get; }
    public string ArtifactsDirectory { get; }

    public LocalTemporaryWorkspace()
    {
        var tmpPath = Path.GetTempFileName();
        File.Delete(tmpPath);
        RootDirectory = tmpPath;
        Directory.CreateDirectory(tmpPath);

        SourceDirectory = Path.Combine(RootDirectory, "s");
        Directory.CreateDirectory(SourceDirectory);

        ArtifactsDirectory = Path.Combine(RootDirectory, "a");
        Directory.CreateDirectory(ArtifactsDirectory);
    }

    public void Dispose()
    {
        //if (RootDirectory.StartsWith(Path.GetTempPath()))
        //{
        //    try
        //    {
        //        Directory.Delete(RootDirectory, recursive: true);
        //    }
        //    catch
        //    {
        //    }
        //}
    }
}