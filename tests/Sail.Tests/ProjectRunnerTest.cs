using Sail.Projects;
using Sail.Workspaces;

namespace Sail.Tests;

public class ProjectRunnerTest
{
    private static SailRunOptions CreateOptions(
        string? configuration = null,
        string? launchProfile = null,
        bool? noLaunchProfile = null,
        string[]? arguments = null)
        => SailRunOptions.Default with
        {
            Configuration = configuration,
            LaunchProfile = launchProfile,
            NoLaunchProfile = noLaunchProfile,
            Arguments = arguments,
        };

    [Fact]
    public void BuildArguments_NoConfiguration()
    {
        var project = new CSharpProjectProject("App.csproj");

        var args = DotNetRunRunner.CreateDotNetBuildArguments(CreateOptions(), project);

        Assert.Equal(["App.csproj"], args);
    }

    [Fact]
    public void BuildArguments_WithConfiguration()
    {
        var project = new CSharpProjectProject("App.csproj");

        var args = DotNetRunRunner.CreateDotNetBuildArguments(CreateOptions(configuration: "Release"), project);

        Assert.Equal(["App.csproj", "--configuration", "Release"], args);
    }

    [Fact]
    public void BuildArguments_FileBased_SameShapeAsProject()
    {
        var project = new FileBasedCSharpSourceProject("Program.cs");

        var args = DotNetRunRunner.CreateDotNetBuildArguments(CreateOptions(configuration: "Release"), project);

        Assert.Equal(["Program.cs", "--configuration", "Release"], args);
    }

    [Fact]
    public void RunArguments_Project_UsesProjectFlag()
    {
        var project = new CSharpProjectProject("App.csproj");

        var args = DotNetRunRunner.CreateDotNetRunArguments(CreateOptions(), project);

        Assert.Equal(["--project", "App.csproj", "--no-build"], args);
    }

    [Fact]
    public void RunArguments_GeneratedProject_UsesProjectFlag()
    {
        var project = new GeneratedCSharpProjectProject(Path.GetTempPath());

        var args = DotNetRunRunner.CreateDotNetRunArguments(CreateOptions(), project);

        Assert.Equal(["--project", project.ProjectPath, "--no-build"], args);
    }

    [Fact]
    public void RunArguments_FileBased_UsesFileFlag()
    {
        var project = new FileBasedCSharpSourceProject("Program.cs");

        var args = DotNetRunRunner.CreateDotNetRunArguments(CreateOptions(), project);

        Assert.Equal(["--file", "Program.cs", "--no-build"], args);
    }

    [Fact]
    public void RunArguments_WithConfiguration_AppliesToRunStep()
    {
        var project = new FileBasedCSharpSourceProject("Program.cs");

        var args = DotNetRunRunner.CreateDotNetRunArguments(CreateOptions(configuration: "Release"), project);

        Assert.Equal(["--file", "Program.cs", "--no-build", "--configuration", "Release"], args);
    }

    [Fact]
    public void RunArguments_WithLaunchProfile()
    {
        var project = new CSharpProjectProject("App.csproj");

        var args = DotNetRunRunner.CreateDotNetRunArguments(CreateOptions(launchProfile: "https"), project);

        Assert.Equal(["--project", "App.csproj", "--no-build", "--launch-profile", "https"], args);
    }

    [Fact]
    public void RunArguments_WithNoLaunchProfile()
    {
        var project = new CSharpProjectProject("App.csproj");

        var args = DotNetRunRunner.CreateDotNetRunArguments(CreateOptions(noLaunchProfile: true), project);

        Assert.Equal(["--project", "App.csproj", "--no-build", "--no-launch-profile"], args);
    }

    [Fact]
    public void RunArguments_WithApplicationArguments_AddsDashDashSeparator()
    {
        var project = new FileBasedCSharpSourceProject("Program.cs");

        var args = DotNetRunRunner.CreateDotNetRunArguments(CreateOptions(arguments: ["--foo", "bar"]), project);

        Assert.Equal(["--file", "Program.cs", "--no-build", "--", "--foo", "bar"], args);
    }

    [Fact]
    public void RunArguments_WithoutApplicationArguments_NoDashDashSeparator()
    {
        var project = new FileBasedCSharpSourceProject("Program.cs");

        var args = DotNetRunRunner.CreateDotNetRunArguments(CreateOptions(arguments: []), project);

        Assert.Equal(["--file", "Program.cs", "--no-build"], args);
    }

    [Fact]
    public void RunArguments_FullOrdering()
    {
        var project = new FileBasedCSharpSourceProject("Program.cs");

        var args = DotNetRunRunner.CreateDotNetRunArguments(
            CreateOptions(configuration: "Release", launchProfile: "https", arguments: ["a", "b"]),
            project);

        Assert.Equal(["--file", "Program.cs", "--no-build", "--configuration", "Release", "--launch-profile", "https", "--", "a", "b"], args);
    }

    [Fact]
    public async Task PublishAndExecRunner_RejectsFileBasedProject()
    {
        var project = new FileBasedCSharpSourceProject("Program.cs");
        var runner = new DotNetPublishAndExecRunner();
        using var workspace = new LocalTemporaryWorkspace();
        var context = new BootstrapContext("Program.cs", null, new Logger(LogLevel.None), CreateOptions(), workspace);

        await Assert.ThrowsAsync<SailExecutionException>(() => runner.RunAsync(context, project));
    }
}
