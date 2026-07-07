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
                        // An explicitly targeted .cs file always runs as a native file-based
                        // application, even if sibling files or a .csproj exist in the same directory.
                        candidates.Add(new FileBasedCSharpSourceProject(targetFullPath));
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
                var sourceFiles = Directory.EnumerateFiles(baseDir, "*.cs").ToArray();
                if (sourceFiles.Length == 1)
                {
                    // A single loose .cs file runs as a native file-based application.
                    candidates.Add(new FileBasedCSharpSourceProject(sourceFiles[0]));
                }
                else if (sourceFiles.Length > 1)
                {
                    // Multiple loose .cs files (with or without Program.cs) are compiled
                    // together via a generated compatibility project.
                    candidates.Add(new GeneratedCSharpProjectProject(baseDir));
                }
            }
        }
    }

}