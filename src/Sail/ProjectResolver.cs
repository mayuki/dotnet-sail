using Sail.Projects;

namespace Sail;

public static class ProjectResolver
{
    public static bool TryFindProjects(string sourceDirectory, string? targetPath, out IReadOnlyList<ISourceProject> candidateProjects)
    {
        var candidates = new List<ISourceProject>();

        if (targetPath is { Length: > 0 })
        {
            // Target=...
            var targetFullPath = Path.Combine(sourceDirectory, targetPath);
            if (File.Exists(targetFullPath))
            {
                // File
                switch (Path.GetExtension(targetFullPath))
                {
                    case ".cs":
                        candidates.Add(new SingleCSharpSourceProject(targetFullPath));
                        break;
                    case ".csproj":
                        candidates.Add(new CSharpProjectProject(targetFullPath));
                        break;
                }
            }
            else if (Directory.Exists(targetFullPath))
            {
                // Directory
                EnumerateFilesInDirectory(candidates, targetFullPath);
            }
        }
        else
        {
            // Target=null
            EnumerateFilesInDirectory(candidates, sourceDirectory);
        }

        candidateProjects = candidates;
        return candidates.Any();

        static void EnumerateFilesInDirectory(List<ISourceProject> candidates, string baseDir)
        {
            // *.csproj
            var csProjs = Directory.EnumerateFiles(baseDir, "*.csproj")
                .Select(x => new CSharpProjectProject(x))
                .ToArray();
            candidates.AddRange(csProjs);

            if (csProjs.Length == 0)
            {
                // *.cs
                var sourceProjects = Directory.EnumerateFiles(baseDir, "*.cs")
                    .Select(x => new SingleCSharpSourceProject(x))
                    .ToArray();

                // Program.cs
                var programCsProject = sourceProjects.SingleOrDefault(x => string.Equals(Path.GetFileName(x.SourcePath), "Program.cs", StringComparison.OrdinalIgnoreCase));
                if (programCsProject is not null)
                {
                    candidates.Add(programCsProject);
                }
                else
                {
                    candidates.AddRange(sourceProjects);
                }
            }
        }
    }

}