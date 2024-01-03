using Sail.Projects;

namespace Sail.Tests;

public class ProjectResolverTest
{
    [Fact]
    public void Empty()
    {
        var targetPath = default(string);
        using var tempDir = new TemporaryDirectory();

        var result = ProjectResolver.TryFindProjects(tempDir.DirectoryPath, targetPath, out var projects);

        Assert.False(result);
    }

    [Fact]
    public void CsFile_Single()
    {
        var targetPath = default(string);
        using var tempDir = new TemporaryDirectory();
        tempDir.AddFile("Program.cs", "");

        var result = ProjectResolver.TryFindProjects(tempDir.DirectoryPath, targetPath, out var projects);

        Assert.True(result);
        Assert.Single(projects);
        Assert.IsType<SingleCSharpSourceProject>(projects[0]);
        Assert.Equal(Path.GetFullPath(Path.Combine(tempDir.DirectoryPath, "App.csproj")),  Path.GetFullPath(projects[0].ProjectPath));
    }

    [Fact]
    public void CsFile_Multiple()
    {
        var targetPath = default(string);
        using var tempDir = new TemporaryDirectory();
        tempDir.AddFile("Program.cs", "");
        tempDir.AddFile("Class1.cs", "");

        var result = ProjectResolver.TryFindProjects(tempDir.DirectoryPath, targetPath, out var projects);

        Assert.True(result);
        Assert.Single(projects);
        Assert.IsType<SingleCSharpSourceProject>(projects[0]);
        Assert.Equal(Path.GetFullPath(Path.Combine(tempDir.DirectoryPath, "App.csproj")),  Path.GetFullPath(projects[0].ProjectPath));
    }
    
    [Fact]
    public void CsFile_With_TargetPath()
    {
        var targetPath = "ConsoleApp1/Program.cs";
        using var tempDir = new TemporaryDirectory();
        tempDir.AddFile("ConsoleApp1/Program.cs", "");
        tempDir.AddFile("ConsoleApp2/Program.cs", "");
        tempDir.AddFile("ConsoleApp3/Program.cs", "");

        var result = ProjectResolver.TryFindProjects(tempDir.DirectoryPath, targetPath, out var projects);

        Assert.True(result);
        Assert.Single(projects);
        Assert.IsType<SingleCSharpSourceProject>(projects[0]);
        Assert.Equal(Path.GetFullPath(Path.Combine(tempDir.DirectoryPath, "ConsoleApp1/App.csproj")),  Path.GetFullPath(projects[0].ProjectPath));
    }

    [Fact]
    public void CsProj()
    {
        var targetPath = default(string);
        using var tempDir = new TemporaryDirectory();
        tempDir.AddFile("Program.cs", "");
        tempDir.AddFile("ConsoleApp1.csproj", "");

        var result = ProjectResolver.TryFindProjects(tempDir.DirectoryPath, targetPath, out var projects);

        Assert.True(result);
        Assert.Single(projects);
        Assert.IsType<CSharpProjectProject>(projects[0]);
    }

    [Fact]
    public void CsProj_Multiple()
    {
        var targetPath = default(string);
        using var tempDir = new TemporaryDirectory();
        tempDir.AddFile("Program.cs", "");
        tempDir.AddFile("ConsoleApp1.csproj", "");
        tempDir.AddFile("ConsoleApp2.csproj", "");

        var result = ProjectResolver.TryFindProjects(tempDir.DirectoryPath, targetPath, out var projects);

        Assert.True(result);
        Assert.Equal(2, projects.Count);
        Assert.IsType<CSharpProjectProject>(projects[0]);
        Assert.IsType<CSharpProjectProject>(projects[1]);
    }
    
    [Fact]
    public void CsProj_With_TargetPath()
    {
        var targetPath = "src/MyApp.Server/MyApp.Server.csproj";

        using var tempDir = new TemporaryDirectory();
        tempDir.AddFile("src/MyApp.Console/Program.cs", "");
        tempDir.AddFile("src/MyApp.Console/MyApp.Console.csproj", "");
        tempDir.AddFile("src/MyApp.Server/Program.cs", "");
        tempDir.AddFile("src/MyApp.Server/MyApp.Server.csproj", "");

        var result = ProjectResolver.TryFindProjects(tempDir.DirectoryPath, targetPath, out var projects);

        Assert.True(result);
        Assert.Single(projects);
        Assert.IsType<CSharpProjectProject>(projects[0]);
        Assert.Equal(Path.GetFullPath(Path.Combine(tempDir.DirectoryPath, "src/MyApp.Server/MyApp.Server.csproj")), Path.GetFullPath(projects[0].ProjectPath));
    }
}