namespace Sail.Tests;

public class GeneratedRegexTest
{
    [Fact]
    public void GistUrl()
    {
        Assert.Matches(GeneratedRegex.GistUrl(), "https://gist.github.com/mayuki/d052d7457a63f25763ce8ecf04b1d0fc");
        Assert.DoesNotMatch(GeneratedRegex.GistUrl(), "https://gist.github.com/d052d7457a63f25763ce8ecf04b1d0fc.git");
        Assert.DoesNotMatch(GeneratedRegex.GistUrl(), "https://gist.github.com/mayuki/d052d7457a63f25763ce8ecf04b1d0fc/archive/763b19d9ea82d3243caa40b79128fb1c347164e5.zip");
    }

    [Fact]
    public void GitUrl()
    {
        Assert.Matches(GeneratedRegex.GitUrl(), "https://github.com/mayuki/dotnet-sail.git");
        Assert.Matches(GeneratedRegex.GitUrl(), "git://github.com/mayuki/dotnet-sail.git");
        Assert.Matches(GeneratedRegex.GitUrl(), "https://gist.github.com/d052d7457a63f25763ce8ecf04b1d0fc.git");
        Assert.Matches(GeneratedRegex.GitUrl(), "https://github.com/mayuki/dotnet-sail.git?path=samples/BlazorWebApp");
        Assert.Matches(GeneratedRegex.GitUrl(), "https://github.com/mayuki/dotnet-sail.git?branch=main");
        Assert.Matches(GeneratedRegex.GitUrl(), "https://github.com/mayuki/dotnet-sail.git?path=samples/BlazorWebApp&branch=main");

        Assert.DoesNotMatch(GeneratedRegex.GitUrl(), "http://github.com/mayuki/dotnet-sail");
    }

    
    [Fact]
    public void GitHubUrl()
    {
        Assert.Matches(GeneratedRegex.GitHubUrl(), "https://github.com/mayuki/dotnet-sail");
        Assert.Matches(GeneratedRegex.GitHubUrl(), "https://github.com/mayuki/dotnet-sail/tree/main/samples/BlazorWebApp");
        Assert.Matches(GeneratedRegex.GitHubUrl(), "https://github.com/mayuki/dotnet-sail/blob/main/samples/BlazorWebApp/BlazorWebApp.csproj");

        Assert.DoesNotMatch(GeneratedRegex.GitHubUrl(), "git://github.com/mayuki/dotnet-sail.git");
        Assert.DoesNotMatch(GeneratedRegex.GitHubUrl(), "https://github.com/mayuki/dotnet-sail/issues");
        Assert.DoesNotMatch(GeneratedRegex.GitHubUrl(), "https://gist.github.com/mayuki/d052d7457a63f25763ce8ecf04b1d0fc");
        Assert.DoesNotMatch(GeneratedRegex.GitHubUrl(), "http://github.com.example.com/mayuki/dotnet-sail");
    }
}